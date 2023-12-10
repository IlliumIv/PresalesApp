using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Radzen.Blazor.Rendering;
using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using PresalesApp.Service;

namespace PresalesApp.Web.Client.Pages;

partial class PresalesNewYearDashboard
{
    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    #region UriQuery
    private const string q_keyword = "Keyword";
    [SupplyParameterFromQuery(Name = q_keyword)] public string? Keyword { get; set; }

    private const string q_start = "Start";
    [SupplyParameterFromQuery(Name = q_start)] public string? Start { get; set; }

    private const string q_end = "End";
    [SupplyParameterFromQuery(Name = q_end)] public string? End { get; set; }

    private const string q_department = "Department";
    [SupplyParameterFromQuery(Name = q_department)] public string? DepartmentType { get; set; }

    private const string q_keyword_type = "KeywordType";
    [SupplyParameterFromQuery(Name = q_keyword_type)] public string? KeywordType { get; set; }

    private Dictionary<string, object?> GetQueryKeyValues() => new()
    {
        [q_start] = _Period.Start.ToString(Helper.UriDateTimeFormat),
        [q_end] = _Period.End.ToString(Helper.UriDateTimeFormat),
        [q_keyword] = _ImageKeyword,
        [q_department] = _Department.ToString(),
        [q_keyword_type] = _KeywordType.ToString(),
    };
    #endregion

    private decimal _DayProfit = 0;
    private List<Presale> _SortedPresales;
    private Department _Department = Department.Russian;
    private ImageKeywordType _KeywordType = ImageKeywordType.Query;
    private readonly Helpers.Period _Period = new(new(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc), PeriodType.Quarter);
    private ImageResponse _Img;
    private string _ImageKeyword = "woman";
    private static ProfitOverview _Overview;
    private readonly PeriodicTimer _PeriodicTimer = new(TimeSpan.FromSeconds(1));
    private string _AerationWarning = "none";
    private TimeSpan _TimeLeft = new(0, 10, 0);
    private string _SelectedSlide = "_ProfitOverview";
    private static string _GetImageSrc(string imageBytes) => $"data:image/png;base64, {imageBytes}";

    private readonly List<_Arrival> _Arrivals = [new _Arrival("Some name", DateTime.Now, "someimage")];

    private static bool _IsRealisticPlanDone() => (_Overview?.Profit.Values.Sum(amount => amount) ?? 0) > 120_000_000;
    private static string _GetHeaderCellStyling() => "text-align: right";
    private static string _GetProgressPercentString(DecimalValue? a, DecimalValue? b) => $"{(decimal)a / (decimal)b * 100:N0}%";

    protected override async Task OnInitializedAsync()
    {
        Helper.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref _Period.Start);
        Helper.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref _Period.End);
        Helper.SetFromQueryOrStorage(value: Keyword, query: q_keyword, uri: Navigation.Uri, storage: Storage, param: ref _ImageKeyword);
        Helper.SetFromQueryOrStorage(value: DepartmentType, query: q_department, uri: Navigation.Uri, storage: Storage, param: ref _Department);
        Helper.SetFromQueryOrStorage(value: KeywordType, query: q_keyword_type, uri: Navigation.Uri, storage: Storage, param: ref _KeywordType);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

        _RunTimer();
        await _ArrivalsStream();
    }

    private async void _RunTimer()
    {
        var count = 600;
        var hour = new TimeSpan(1, 0, 0);
        while(await _PeriodicTimer.WaitForNextTickAsync())
        {
            if(count > 600)
            {
                count = 0;
                await _UpdateData();
                await _UpdateImage();
                await _RedrawChart();
            }

            count++;
            var currentTime = DateTime.Now.TimeOfDay;
            _TimeLeft = hour - new TimeSpan(0, currentTime.Minutes, currentTime.Seconds);

            _AerationWarning = currentTime.Minutes switch
            {
                > 49 => currentTime.Hours switch
                {
                    10 or 12 or 14 or 16 => "flex",
                    _ => "none"
                },
                _ => "none"
            };

            StateHasChanged();
        }
    }

    private async Task _UpdateData()
    {
        try
        {
            _Overview = await AppApi.GetProfitOverviewAsync(new OverviewRequest
            {
                Period = _Period.Translate(),
                Department = _Department,
                Position = Position.Any
            });

            _SortedPresales = [.. _Overview.Presales.OrderByDescending(p => p.Statistics.Profit)];
            _DayProfit = _Overview.Profit
                .FirstOrDefault(p => p.Key == DateTime.Now.Date.ToUniversalTime()
                .ToString(CultureInfo.InvariantCulture)).Value ?? 0;
        }
        catch
        {
            await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
            return;
        }
    }

    private async Task _ArrivalsStream()
    {
        while(true)
        {
            try
            {
                var call = BridgeApi.GetPresalesArrival(new Empty());
                var token = new CancellationToken();

                while(await call.ResponseStream.MoveNext(token))
                {
                    var arrival = call.ResponseStream.Current;
                    var dt = arrival.Timestamp.ToDateTime();

                    var a = _Arrivals.FirstOrDefault(a => a?.Name == arrival.Name);

                    if(a is not null)
                    {
                        if(a.Timestamp.Date < dt.Date)
                        {
                            a.Timestamp = dt;
                            a.ImageBytes = arrival.ImageBytes;
                        }
                    }
                    else
                    {
                        _Arrivals.Add(new _Arrival(arrival.Name, dt, arrival.ImageBytes));
                    }

                    _Arrivals.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

                    StateHasChanged();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private async Task _UpdateImage()
    {
        try
        {
            _Img = await AppApi.GetImageAsync(new ImageRequest
            {
                Keyword = _ImageKeyword,
                Orientation = ImageOrientation.Portrait,
                KeywordType = _KeywordType
            });
        }
        catch
        {
            await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
            return;
        }
    }

    private async void _OnManuallyImageUpdate(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter")
        {
            Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{q_keyword}", _ImageKeyword);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            await _UpdateImage();
        }
    }

    private void _OnImageKeywordTypeChange()
    {
        _KeywordType = _KeywordType switch
        {
            ImageKeywordType.Query => ImageKeywordType.Collections,
            ImageKeywordType.Collections => ImageKeywordType.Query,
            _ => throw new NotImplementedException()
        };
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_keyword_type}", _KeywordType);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
    }

    #region Charts
    private static LineChart<decimal> _LineChart;

    private static string _GetChartOptions() => "{\"aspectRatio\":2.43875, \"plugins\":{\"legend\":{\"display\": false}}}";
    private static string _GetInvoicesChartOptions() => "{\"cutout\":\"80%\",\"animation\":{\"animateScale\": true}}";

    private static LineChartDataset<decimal> _GetChartDataset(List<decimal> data) => new()
    {
        Label = "Сумма",
        Data = data,
        BackgroundColor = GetColors(0.3f, max: 1),
        BorderColor = GetColors(0.8f, max: 1),
        Fill = true,
        PointRadius = 1,
    };

    private static LineChartDataset<decimal> _GetRealisticPlanDataset(List<decimal> data) => new()
    {
        Label = "Реалистичный план",
        Data = data,
        BorderColor = GetColors(0.8f),
        PointRadius = 0,
    };

    private static LineChartDataset<decimal> _GetAmbitiousPlanDataset(List<decimal> data) => new()
    {
        Label = "Амбициозный план",
        Data = data,
        BorderColor = GetColors(0.8f),
        PointRadius = 0,
    };

    private static LineChartDataset<decimal> _GetMinPointDataset() => new()
    {
        Label = "min point",
        Data = [155_000_000],
        PointRadius = 0,
    };

    #region Colors
    private static readonly (byte R, byte G, byte B)[] colors = [(12, 90, 74),
        (5, 47, 91),
        (165, 14, 130),
        (232, 125, 55),
        (106, 158, 31),
        (20, 150, 124),
        (99, 8, 78),
        (198, 35, 36),
        (3, 28, 58)];

    private static List<string> GetColors(float alfa, int max = int.MaxValue)
    {
        var r = new List<string>();

        for (int i = 0; i < colors.Length && i < max; i++)
        {
            r.Add(ChartColor.FromRgba(colors[i].R, colors[i].G, colors[i].B, alfa));
        }

        return r;
    }
    #endregion

    private async Task _RedrawChart()
    {
        if(_Overview == null)
        {
            return;
        }

        await _LineChart.Clear();

        var labels = new List<string>();
        var profit = new List<decimal>();
        var realistic_plan = new List<decimal>();
        var ambitious_plan = new List<decimal>();
        decimal amount = 0;

        foreach ((var date, var day_profit) in _Overview.Profit)
        {
            var dt = DateTime.Parse(date, CultureInfo.InvariantCulture).ToLocalTime();
            if (dt > DateTime.Now) break;
            amount += day_profit;
            labels.Add(dt.GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization));
            profit.Add(amount);
            realistic_plan.Add(120_000_000);
            ambitious_plan.Add(150_000_000);
        }

        var invoices = new List<int>();
        foreach (var presale in _Overview.Presales.OrderByDescending(p => p.Statistics.Profit))
        {
            invoices.Add(presale.Statistics.InvoicesShipped);
        }

        await _LineChart.AddLabelsDatasetsAndUpdate(labels: labels,
            _GetChartDataset(profit),
            _GetRealisticPlanDataset(realistic_plan),
            _GetAmbitiousPlanDataset(ambitious_plan),
            _GetMinPointDataset());
    }
    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _PeriodicTimer?.Dispose();
    }

    class _Arrival(string name, DateTime timestamp, string imageBytes)
    {
        public string Name { get; } = name;
        public DateTime Timestamp { get; set; } = timestamp;
        public string ImageBytes { get; set; } = imageBytes;
    }
}

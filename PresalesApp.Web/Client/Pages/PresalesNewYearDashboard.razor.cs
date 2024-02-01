using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using pax.BlazorChartJs;

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
    private static bool _IsLate => DateTime.UtcNow.TimeOfDay > TimeSpan.FromHours(5);
    private static string _GetImageSrc(string imageBytes) => $"data:image/png;base64, {imageBytes}";
    private readonly CancellationTokenSource _ArrivalsStreamCancelTokenSource = new();

    private List<_Arrival> _Arrivals = [];

    private static bool _IsRealisticPlanDone() => (_Overview?.Profit.Values.Sum(amount => amount) ?? 0) > _Overview?.Actual;
    private static string _GetHeaderCellStyling() => "text-align: right";
    private static string _GetProgressPercentString(DecimalValue? a, DecimalValue? b) => $"{(decimal)a / (decimal)b * 100:N0}%";
    private static string _GetProgressPercentString(TimeSpan a, TimeSpan b) => $"{a / b * 100:N0}%";

    protected override void OnInitialized()
    {
        Helper.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref _Period.Start);
        Helper.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref _Period.End);
        Helper.SetFromQueryOrStorage(value: Keyword, query: q_keyword, uri: Navigation.Uri, storage: Storage, param: ref _ImageKeyword);
        Helper.SetFromQueryOrStorage(value: DepartmentType, query: q_department, uri: Navigation.Uri, storage: Storage, param: ref _Department);
        Helper.SetFromQueryOrStorage(value: KeywordType, query: q_keyword_type, uri: Navigation.Uri, storage: Storage, param: ref _KeywordType);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

        _RunTimer();
        _ArrivalsStream(_ArrivalsStreamCancelTokenSource.Token);
    }

    private async void _RunTimer()
    {
        var count = 600;
        var hour = new TimeSpan(1, 0, 0);
        while(await _PeriodicTimer.WaitForNextTickAsync())
        {
            if(count > 600)
            {
                if(_IsLate)
                {
                    _SelectedSlide = "_ProfitOverview";
                }

                count = 0;
                await _UpdateData();
                await _UpdateImage();
                _RedrawChart();
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
            GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
            return;
        }
    }

    private async void _ArrivalsStream(CancellationToken token)
    {
        while(true && !_IsLate && !token.IsCancellationRequested)
        {
            try
            {
                var call = BridgeApi.GetPresalesArrival(new Empty(), cancellationToken: token);

                while(await call.ResponseStream.MoveNext(token).ConfigureAwait(false))
                    _AddOrUpdateArrival(call.ResponseStream.Current);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private void _AddOrUpdateArrival(Service.Arrival arrival)
    {
        var dt = arrival.Timestamp.ToDateTime().ToLocalTime();

        if(_Arrivals.Any(a => a?.Timestamp.Date < dt.Date))
            _Arrivals.Clear();

        if(!_Arrivals.Exists(a => a?.Name == arrival.Name))
            _Arrivals.Add(new _Arrival(arrival.Name, dt, arrival.ImageBytes));

        _Arrivals = [.. _Arrivals.OrderBy(a => a?.Timestamp)];

        StateHasChanged();
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
            GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
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
    private readonly ChartJsConfig _LineChartConfig = ChartHelpers.GenerateChartConfig(chartType: ChartType.line,
        options: new() { AspectRatio = 2.43875, Plugins = new() { Legend = new() { Display = false  }}});

    private void _RedrawChart()
    {
        if(_Overview == null) return;

        var labels = new List<string>();
        var profit = new List<object>();
        decimal amount = 0;

        foreach ((var date, var day_profit) in _Overview.Profit)
        {
            var dt = DateTime.Parse(date, CultureInfo.InvariantCulture).ToLocalTime();
            if (dt > DateTime.Now) break;
            amount += day_profit;
            labels.Add(dt.GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization));
            profit.Add(amount);
        }

        var invoices = new List<int>();
        foreach (var presale in _Overview.Presales.OrderByDescending(p => p.Statistics.Profit))
        {
            invoices.Add(presale.Statistics.InvoicesShipped);
        }

        // Plan lines always must be
        if (labels.Count == 1) labels.Add(labels.First());

        _LineChartConfig.Update(labels: labels,
            ChartHelpers.GetLineDataset(profit, "Сумма", "DeepGreen", 0.3f, 0.8f),
            ChartHelpers.GetLineDataset(ChartHelpers.GenerateLine((ushort)labels.Count, (decimal)_Overview.Actual),
                "Реалистичный план", "DeepBlue", 0.3f, 0.8f, false, 0),
            ChartHelpers.GetLineDataset(ChartHelpers.GenerateLine((ushort)labels.Count, (decimal)_Overview.Plan),
                "Амбициозный план", "DeepPurple", 0.3f, 0.8f, false, 0),
            ChartHelpers.GetLineDataset(ChartHelpers.GenerateLine(1, _Overview.Plan * (decimal)1.03),
                "min_point", null, null, null, false, 0));
    }
    #endregion

    public void Dispose()
    {
        _ArrivalsStreamCancelTokenSource.Cancel();
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

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Client.Extensions;
using PresalesApp.Shared;
using PresalesApp.CustomTypes;
using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using pax.BlazorChartJs;
using PresalesApp.ImageProvider;
using PresalesApp.Extensions;
using Radzen;

namespace PresalesApp.Web.Client.Pages;

partial class PresalesNewYearDashboard
{
    [Inject]
    private NotificationService _NotificationService { get; set; }

    #region UriQuery

    private const string _Q_Keyword = "Keyword";
    [SupplyParameterFromQuery(Name = _Q_Keyword)]
    public string? Keyword { get; set; }

    private const string _Q_Start = "Start";
    [SupplyParameterFromQuery(Name = _Q_Start)]
    public string? Start { get; set; }

    private const string _Q_End = "End";
    [SupplyParameterFromQuery(Name = _Q_End)]
    public string? End { get; set; }

    private const string _Q_DepartmentType = "Department";
    [SupplyParameterFromQuery(Name = _Q_DepartmentType)]
    public string? DepartmentType { get; set; }

    private const string _Q_KeywordType = "KeywordType";
    [SupplyParameterFromQuery(Name = _Q_KeywordType)]
    public string? KeywordType { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_Q_Start] = _Period.Start.ToString(Helpers.UriDateTimeFormat),
        [_Q_End] = _Period.End.ToString(Helpers.UriDateTimeFormat),
        [_Q_Keyword] = _ImageKeyword,
        [_Q_DepartmentType] = _Department.ToString(),
        [_Q_KeywordType] = _KeywordType.ToString(),
    };

    #endregion

    private decimal _DayProfit = 0;

    private List<Presale> _SortedPresales;

    private Department _Department = Department.Russian;

    private KeywordType _KeywordType = ImageProvider.KeywordType.Query;

    private readonly Extensions.Period _Period = new(new(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc),
        PeriodType.Quarter);

    private GetImageResponse _Img;

    private string _ImageKeyword = "woman";

    private static GetProfitOverviewResponse _Overview;

    private readonly PeriodicTimer _PeriodicTimer = new(TimeSpan.FromSeconds(1));

    private string _AerationWarning = "none";

    private TimeSpan _TimeLeft = new(0, 10, 0);

    private string _OverviewDisableClass = "";

    private string _ArrivalDisableClass = "disable";

    private static bool _IsLate => DateTime.UtcNow.TimeOfDay > TimeSpan.FromHours(5);

    // private static string _GetImageSrc(string imageBytes) => $"data:image/png;base64, {imageBytes}";

    private readonly CancellationTokenSource _ArrivalsStreamCancelTokenSource = new();

    private List<_Arrival> _Arrivals = [];

    private static bool _IsRealisticPlanDone()
        => (_Overview?.Profit.Values.Sum(amount => amount) ?? 0) > _Overview?.Actual;

    private static bool _IsAmbitiousPlanDone()
        => (_Overview?.Profit.Values.Sum(amount => amount) ?? 0) > _Overview?.Plan;

    // private static string _GetHeaderCellStyling() => "text-align: right";

    private static string _GetProgressPercentString(DecimalValue? a, DecimalValue? b)
    {
        var d = (decimal)a / (decimal)b * 100;
        return string.Format($"{{0:N0}}%", d);
    }

    private static string _GetProgressPercentString(TimeSpan a, TimeSpan b) => $"{a / b * 100:N0}%";

    protected override void OnInitialized()
    {
        Helpers.SetFromQueryOrStorage(value: Start, query: _Q_Start,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Start);
        Helpers.SetFromQueryOrStorage(value: End, query: _Q_End,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.End);
        Helpers.SetFromQueryOrStorage(value: Keyword, query: _Q_Keyword,
            uri: Navigation.Uri, storage: Storage, param: ref _ImageKeyword);
        Helpers.SetFromQueryOrStorage(value: DepartmentType, query: _Q_DepartmentType,
            uri: Navigation.Uri, storage: Storage, param: ref _Department);
        Helpers.SetFromQueryOrStorage(value: KeywordType, query: _Q_KeywordType,
            uri: Navigation.Uri, storage: Storage, param: ref _KeywordType);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        _RunTimer();
        _ArrivalsStream(_ArrivalsStreamCancelTokenSource.Token);
    }

    private async void _RunTimer()
    {
        var count = 600;
        var hour = new TimeSpan(1, 0, 0);
        while(await _PeriodicTimer.WaitForNextTickAsync())
        {
            if (count % 30 == 0)
            {
                if (_IsLate || _ArrivalDisableClass == "")
                {
                    _ArrivalDisableClass = "disable";
                    _OverviewDisableClass = "";
                }
                else
                {
                    _ArrivalDisableClass = "";
                    _OverviewDisableClass = "disable";
                }
            }

            if(count > 600)
            {
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
            _Overview = await PresalesAppApi.GetProfitOverviewAsync(new()
            {
                Period = _Period.Translate(),
                Department = _Department,
                Position = Position.Any
            });

            _SortedPresales = [.. _Overview.Presales.OrderByDescending(p => p.Metrics.Profit)];
            _DayProfit = _Overview.Profit
                .FirstOrDefault(p => p.Key == DateTime.Now.Date.ToUniversalTime()
                .ToString(CultureInfo.InvariantCulture)).Value ?? 0;
        }
        catch
        {
            _NotificationService.Notify(NotificationSeverity.Error,
                Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
            return;
        }
    }

    private async void _ArrivalsStream(CancellationToken token)
    {
        var timeout = DateTime.Now.AddMinutes(-5);

        while(timeout.AddMinutes(5) < DateTime.Now && !_IsLate && !token.IsCancellationRequested)
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

            timeout = DateTime.Now;
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
            _Img = await ImageProviderApi.GetImageAsync(new()
            {
                Keyword = _ImageKeyword,
                Orientation = ImageProvider.Orientation.Portrait,
                KeywordType = _KeywordType
            });
        }
        catch
        {
            _NotificationService.Notify(NotificationSeverity.Error,
                Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
            return;
        }
    }

    private async void _OnManuallyImageUpdate(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter")
        {
            Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{_Q_Keyword}", _ImageKeyword);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
            await _UpdateImage();
        }
    }

    private void _OnImageKeywordTypeChange()
    {
        _KeywordType = _KeywordType.Next();
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_KeywordType}", _KeywordType);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
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
        foreach (var presale in _Overview.Presales.OrderByDescending(p => p.Metrics.Profit))
        {
            invoices.Add(presale.Metrics.InvoicesShipped);
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

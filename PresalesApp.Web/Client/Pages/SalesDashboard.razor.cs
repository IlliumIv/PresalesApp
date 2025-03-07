using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Shared;
using PresalesApp.CustomTypes;
using Radzen;
using pax.BlazorChartJs;
using PresalesApp.Web.Client.Extensions;

namespace PresalesApp.Web.Client.Pages;

partial class SalesDashboard
{
    [Inject]
    private NotificationService _NotificationService { get; set; }

    #region UriQuery
    private const string _Q_PrevStart = "pStart";
    [SupplyParameterFromQuery(Name = _Q_PrevStart)]
    public string? PrevStart { get; set; }

    private const string _Q_PrevEnd = "pEnd";
    [SupplyParameterFromQuery(Name = _Q_PrevEnd)]
    public string? PrevEnd { get; set; }

    private const string _Q_PrevPeriodType = "pPeriod";
    [SupplyParameterFromQuery(Name = _Q_PrevPeriodType)]
    public string? PrevPeriodType { get; set; }

    private const string _Q_CurrStart = "cStart";
    [SupplyParameterFromQuery(Name = _Q_CurrStart)]
    public string? CurrStart { get; set; }

    private const string _Q_CurrEnd = "cEnd";
    [SupplyParameterFromQuery(Name = _Q_CurrEnd)]
    public string? CurrEnd { get; set; }

    private const string _Q_CurrPeriodType = "cPeriod";
    [SupplyParameterFromQuery(Name = _Q_CurrPeriodType)]
    public string? CurrPeriodType { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_Q_PrevStart] = _Previous.Start.ToString(Helpers.UriDateTimeFormat),
        [_Q_PrevEnd] = _Previous.End.ToString(Helpers.UriDateTimeFormat),
        [_Q_PrevPeriodType] = _Previous.Type.ToString(),
        [_Q_CurrStart] = _Current.Start.ToString(Helpers.UriDateTimeFormat),
        [_Q_CurrEnd] = _Current.End.ToString(Helpers.UriDateTimeFormat),
        [_Q_CurrPeriodType] = _Current.Type.ToString(),
    };
    #endregion

    private string _TitlePin = "Закрепить";

    private string _ClassHide = "hide";

    private string _ClassRotatePin = "rotatePin";

    private GetSalesOverviewResponse? _OverviewResponse;

    private List<string> _DatasetColors = [];

    private List<string> _BorderColors = [];

    private static DateTime _GetFirstDay()
        => new(DateTime.Now.Year, ((((DateTime.Now.Month - 1) / 3) + 1) * 3) - 2, 1);

    private Extensions.Period _Current = new(_GetFirstDay(), PeriodType.Quarter);

    private Extensions.Period _Previous = new(_GetFirstDay().AddYears(-1), PeriodType.Quarter);

    private readonly ChartJsConfig _PieChartConfig = ChartHelpers.GenerateChartConfig(ChartType.pie);

    private async Task _OnPeriodChanged()
    {
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PrevStart}", _Previous.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PrevEnd}", _Previous.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PrevPeriodType}", _Previous.Type);

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_CurrStart}", _Current.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_CurrEnd}", _Current.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_CurrPeriodType}", _Current.Type);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _HandleRedrawChart();
    }

    private void _PinParams()
    {
        if (_ClassHide.Length > 0)
        {
            _ClassHide = "";
            _TitlePin = "Открепить";
            _ClassRotatePin = "";
        }
        else
        {
            _ClassHide = "hide";
            _TitlePin = "Закрепить";
            _ClassRotatePin = "rotatePin";
        }
    }

    private readonly PeriodicTimer _PeriodicTimer = new(TimeSpan.FromMinutes(10));

    private async Task _UpdateData()
    {
        try
        {
            _OverviewResponse = await PresalesAppApi.GetSalesOverviewAsync(new()
            {
                Previous = _Previous.Translate(),
                Current = _Current.Translate(),
            });
        }
        catch
        {
            _NotificationService.Notify(NotificationSeverity.Error,
                Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
        }
    }

    protected override void OnInitialized()
    {
        Helpers.SetFromQueryOrStorage(value: PrevStart, query: _Q_PrevStart,
            uri: Navigation.Uri, storage: Storage, param: ref _Previous.Start);
        Helpers.SetFromQueryOrStorage(value: PrevEnd, query: _Q_PrevEnd,
            uri: Navigation.Uri, storage: Storage, param: ref _Previous.End);
        Helpers.SetFromQueryOrStorage(value: PrevPeriodType, query: _Q_PrevPeriodType,
            uri: Navigation.Uri, storage: Storage, param: ref _Previous.Type);

        Helpers.SetFromQueryOrStorage(value: CurrStart, query: _Q_CurrStart,
            uri: Navigation.Uri, storage: Storage, param: ref _Current.Start);
        Helpers.SetFromQueryOrStorage(value: CurrEnd, query: _Q_CurrEnd,
            uri: Navigation.Uri, storage: Storage, param: ref _Current.End);
        Helpers.SetFromQueryOrStorage(value: CurrPeriodType, query: _Q_CurrPeriodType,
            uri: Navigation.Uri, storage: Storage, param: ref _Current.Type);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        _RunTimer();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _PeriodicTimer?.Dispose();
    }

    private static string _GetPieChartOptions()
        => "{\"animation\":{\"animateScale\": true},\"plugins\":{\"tooltip\":{\"enabled\": false}}}";

    private async void _RunTimer()
    {
        while (await _PeriodicTimer.WaitForNextTickAsync())
        {
            await _HandleRedrawChart();
        }
    }

    private async Task _HandleRedrawChart()
    {
        if (_Previous.Start > _Previous.End)
        {
            _NotificationService.Notify(NotificationSeverity.Warning,
                "Начало предыдущего периода должно быть меньше окончания!");
            return;
        }

        if (_Current.Start > _Current.End)
        {
            _NotificationService.Notify(NotificationSeverity.Warning,
                "Начало текущего периода должно быть меньше окончания!");
            return;
        }

        await _UpdateData();
        if (_OverviewResponse == null) return;

        _DatasetColors = [];
        _BorderColors = [];

        var colors_alfa = 0.5f;
        var dataset = new List<decimal>();
        var rnd = new Random();

        foreach (var manager in _OverviewResponse.CurrentTopSalesManagers)
        {
            dataset.Add(manager.Profit);
            var r = rnd.Next(255);
            var g = rnd.Next(255);
            var b = rnd.Next(255);
            // _DatasetColors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), colors_alfa));
            // _BorderColors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), 1));
        }

        var sum = _OverviewResponse.CurrentTopSalesManagers.Sum(m => m.Profit);

        if (_OverviewResponse.CurrentActualProfit > sum)
        {
            dataset.Add(_OverviewResponse.CurrentActualProfit - sum);
            var r = rnd.Next(255);
            var g = rnd.Next(255);
            var b = rnd.Next(255);
            // _DatasetColors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), colors_alfa));
            // _BorderColors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), 1));
        }

        if (_OverviewResponse.CurrentSalesTarget > _OverviewResponse.CurrentActualProfit)
        {
            dataset.Add(_OverviewResponse.CurrentSalesTarget - _OverviewResponse.CurrentActualProfit);
            // _DatasetColors.Add(ChartColor.FromRgba(240, 240, 240, colors_alfa));
            // _BorderColors.Add(ChartColor.FromRgba(240, 240, 240, 1));
        }

        _PieChartConfig.Update(null, ChartHelpers.GetPieDataset(dataset));

        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _HandleRedrawChart();
        }
    }
}

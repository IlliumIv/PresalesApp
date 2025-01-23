using Blazorise.DataGrid;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Shared;
using PresalesApp.CustomTypes;

namespace PresalesApp.Web.Client.Pages;

partial class KPI
{
    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    #region Private Members

    private bool _IsBtnDisabled = true;

    private Helpers.Period _Period = new(new(DateTime.Now.Year, DateTime.Now.Month, 1),
        CustomTypes.PeriodType.Month);

    private string _PresaleName = string.Empty;

    private Kpi? _Response;

    private KpiCalculation _KpiCalculationType = KpiCalculation.Default;

    private readonly HashSet<PeriodType> _ExcludedPeriods =
    [
        CustomTypes.PeriodType.Day,
        CustomTypes.PeriodType.Quarter,
        CustomTypes.PeriodType.Year,
        CustomTypes.PeriodType.Arbitrary,
    ];

    #endregion

    #region UriQuery

    private const string _Q_Start = "Start";
    [SupplyParameterFromQuery(Name = _Q_Start)]
    public string? Start { get; set; }

    private const string _Q_End = "End";
    [SupplyParameterFromQuery(Name = _Q_End)]
    public string? End { get; set; }

    private const string _Q_PeriodType = "Period";
    [SupplyParameterFromQuery(Name = _Q_PeriodType)]
    public string? PeriodType { get; set; }

    private const string _Q_PresaleName = "Presale";
    [SupplyParameterFromQuery(Name = _Q_PresaleName)]
    public string? PresaleName { get; set; }

    private const string _Q_CalculationMethod = "Method";
    [SupplyParameterFromQuery(Name = _Q_CalculationMethod)]
    public string? CalculationMethod { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_Q_Start] = _Period.Start.ToString(Helper.UriDateTimeFormat),
        [_Q_End] = _Period.End.ToString(Helper.UriDateTimeFormat),
        [_Q_PeriodType] = _Period.Type.ToString(),
        [_Q_PresaleName] = _PresaleName,
        [_Q_CalculationMethod] = _KpiCalculationType.ToString(),
    };

    #endregion

    protected override async Task OnInitializedAsync()
    {
        Helper.SetFromQueryOrStorage(value: Start, query: _Q_Start,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Start);
        Helper.SetFromQueryOrStorage(value: End, query: _Q_End,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.End);
        Helper.SetFromQueryOrStorage(value: PeriodType, query: _Q_PeriodType,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Type);
        Helper.SetFromQueryOrStorage(value: PresaleName, query: _Q_PresaleName,
            uri: Navigation.Uri, storage: Storage, param: ref _PresaleName);
        Helper.SetFromQueryOrStorage(value: CalculationMethod, query: _Q_CalculationMethod,
            uri: Navigation.Uri, storage: Storage, param: ref _KpiCalculationType);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
        await _GenerateReport();
    }

    #region Private Methods

    private static void _GetRowStyle(Invoice invoice, DataGridRowStyling styling)
        => styling.Style = $"color: {Helper.SetColor(invoice)};";

    private async Task _OnPresaleChanged(string name)
    {
        _PresaleName = name;

        Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PresaleName}", _PresaleName);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _GenerateReport();
    }

    private async Task _OnPeriodChanged(Helpers.Period period)
    {
        _Period = period;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_Start}", _Period.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_End}", _Period.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PeriodType}", _Period.Type);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _GenerateReport();
    }

    private async Task _OnCalcMethodChanged(KpiCalculation method)
    {
        _KpiCalculationType = method;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_CalculationMethod}", _KpiCalculationType);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _GenerateReport();
    }

    private async Task _DownloadReport()
        => await _Response.Download(js,_PresaleName, _Period, Localization);

    private async Task _GenerateReport()
    {
        _Response = null;
        _IsBtnDisabled = true;

        if (string.IsNullOrEmpty(_PresaleName))
        {
            GlobalMsgHandler.Show($"{Localization["NeedSelectPresaleMessageText"]}");
            return;
        }

        var response = await PresalesAppApi.GetKpiAsync(new()
        {
            PresaleName = _PresaleName,
            Period = _Period.Translate(),
            KpiCalculation = _KpiCalculationType
        });

        if (response.KindCase == GetKpiResponse.KindOneofCase.Kpi)
        {
            _Response = response.Kpi;
            _IsBtnDisabled = false;
        }

        (var message, var color) = response.KindCase switch
        {
            GetKpiResponse.KindOneofCase.Error => (response.Error.Message, SnackbarColor.Danger),
            GetKpiResponse.KindOneofCase.Kpi => ($"{Localization["ReportIsDoneMessageText"]}",
                                                 SnackbarColor.Success),
            GetKpiResponse.KindOneofCase.None => ($"{Localization["NoInvoicesForThisPeriodMessageText"]}",
                                                  SnackbarColor.Success),
            _ => ($"{Localization["UnknownServerResponseMessageText"]}", SnackbarColor.Danger)
        };

        GlobalMsgHandler.Show(message, color);
        StateHasChanged();
    }
    #endregion
}

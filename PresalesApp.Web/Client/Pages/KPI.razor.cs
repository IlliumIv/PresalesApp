using Blazorise.DataGrid;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages
{
    partial class KPI
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        #region Private Members
        private bool _is_btn_disabled = true;
        private Period _period = new(new(DateTime.Now.Year, DateTime.Now.Month, 1), Enums.PeriodType.Month);
        private string _presale_name = string.Empty;
        private Kpi? _response;
        private KpiCalculation _KpiCalculationType = KpiCalculation.Default;
        private HashSet<PeriodType> _ExcludedPeriods =
        [
            Enums.PeriodType.Day,
            Enums.PeriodType.Quarter,
            Enums.PeriodType.Year,
            Enums.PeriodType.Arbitrary,
        ];

        #endregion

        #region UriQuery
        private const string q_start = "Start";
        [SupplyParameterFromQuery(Name = q_start)] public string? Start { get; set; }

        private const string q_end = "End";
        [SupplyParameterFromQuery(Name = q_end)] public string? End { get; set; }

        private const string q_period_type = "Period";
        [SupplyParameterFromQuery(Name = q_period_type)] public string? PeriodType { get; set; }

        private const string q_presale = "Presale";
        [SupplyParameterFromQuery(Name = q_presale)] public string? PresaleName { get; set; }

        private const string q_method = "Method";
        [SupplyParameterFromQuery(Name = q_method)] public string? CalculationMethod { get; set; }

        private Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = _period.Start.ToString(Helper.UriDateTimeFormat),
            [q_end] = _period.End.ToString(Helper.UriDateTimeFormat),
            [q_period_type] = _period.Type.ToString(),
            [q_presale] = _presale_name,
            [q_method] = _KpiCalculationType.ToString(),
        };
        #endregion

        protected override async Task OnInitializedAsync()
        {
            Helper.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref _period.Start);
            Helper.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref _period.End);
            Helper.SetFromQueryOrStorage(value: PeriodType, query: q_period_type, uri: Navigation.Uri, storage: Storage, param: ref _period.Type);
            Helper.SetFromQueryOrStorage(value: PresaleName, query: q_presale, uri: Navigation.Uri, storage: Storage, param: ref _presale_name);
            Helper.SetFromQueryOrStorage(value: CalculationMethod, query: q_method, uri: Navigation.Uri, storage: Storage, param: ref _KpiCalculationType);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            await GenerateReport();
        }

        #region Private Methods
        private static void GetRowStyle(Invoice invoice, DataGridRowStyling styling) =>
            styling.Style = $"color: {Helper.SetColor(invoice)};";

        private async Task OnPresaleChanged(string name)
        {
            _presale_name = name;

            Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{q_presale}", _presale_name);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await GenerateReport();
        }

        private async Task OnPeriodChanged(Period period)
        {
            _period = period;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_start}", _period.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_end}", _period.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_period_type}", _period.Type);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await GenerateReport();
        }

        private async Task OnCalcMethodChanged(KpiCalculation method)
        {
            _KpiCalculationType = method;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_method}", _KpiCalculationType);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await GenerateReport();
        }

        private async Task DownloadReport() => await _response.Download(js, _presale_name, _period, Localization);

        private async Task GenerateReport()
        {
            _response = null;
            _is_btn_disabled = true;

            if (string.IsNullOrEmpty(_presale_name))
            {
                GlobalMsgHandler.Show($"{Localization["NeedSelectPresaleMessageText"]}");
                return;
            }

            var response = await AppApi.GetKpiAsync(new KpiRequest
            {
                PresaleName = _presale_name,
                Period = _period.TranslateAsSharedPeriod(),
                KpiCalculationType = _KpiCalculationType
            });

            if (response.ResultCase == KpiResponse.ResultOneofCase.Kpi)
            {
                _response = response.Kpi;
                _is_btn_disabled = false;
            }

            (string message, SnackbarColor color) = response.ResultCase switch
            {
                KpiResponse.ResultOneofCase.Error => (response.Error.Message, SnackbarColor.Danger),
                KpiResponse.ResultOneofCase.Kpi => ($"{Localization["ReportIsDoneMessageText"]}", SnackbarColor.Success),
                KpiResponse.ResultOneofCase.None => ($"{Localization["NoInvoicesForThisPeriodMessageText"]}", SnackbarColor.Success),
                _ => ($"{Localization["UnknownServerResponseMessageText"]}", SnackbarColor.Danger)
            };

            GlobalMsgHandler.Show(message, color);
            StateHasChanged();
        }
        #endregion
    }
}

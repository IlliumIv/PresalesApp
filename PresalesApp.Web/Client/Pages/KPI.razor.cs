﻿using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Period = PresalesApp.Web.Client.Helpers.Period;
using PresalesApp.Web.Client.Helpers;
using Blazorise.Snackbar;

namespace PresalesApp.Web.Client.Pages
{
    partial class KPI
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        #region Private Members
        private bool _is_btn_disabled = true;
        private static Period _period = new(new(DateTime.Now.Year, DateTime.Now.Month, 1), Enums.PeriodType.Month);
        private static string _presale_name = string.Empty;
        private Kpi? _response;
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

        private static Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = _period.Start.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_end] = _period.End.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_period_type] = _period.Type.ToString(),
            [q_presale] = _presale_name,
        };
        #endregion

        protected override async Task OnInitializedAsync()
        {
            Helpers.Helpers.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref _period.Start);
            Helpers.Helpers.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref _period.End);
            Helpers.Helpers.SetFromQueryOrStorage(value: PeriodType, query: q_period_type, uri: Navigation.Uri, storage: Storage, param: ref _period.Type);
            Helpers.Helpers.SetFromQueryOrStorage(value: PresaleName, query: q_presale, uri: Navigation.Uri, storage: Storage, param: ref _presale_name);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            await GenerateReport();
        }

        #region Private Methods
        private static void GetRowStyle(Invoice invoice, DataGridRowStyling styling) =>
            styling.Style = $"color: {Helpers.Helpers.SetColor(invoice)};";

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

        private async Task DownloadReport() => await _response.Download(js, _presale_name, _period, Localization);

        private async Task GenerateReport()
        {
            _response = null;
            _is_btn_disabled = true;

            if (string.IsNullOrEmpty(_presale_name))
            {
                await GlobalMsgHandler.Show($"{Localization["NeedSelectPresaleMessageText"]}");
                return;
            }

            var response = await AppApi.GetKpiAsync(new KpiRequest
            {
                PresaleName = _presale_name,
                Period = _period.Translate()
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

            await GlobalMsgHandler.Show(message, color);
            StateHasChanged();
        }
        #endregion
    }
}
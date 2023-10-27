using Blazorise.DataGrid;
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

        private bool downloadButtonDisabled = true;

        private static void GetRowStyle(Invoice invoice, DataGridRowStyling styling) =>
            styling.Style = $"color: {Helpers.Helpers.SetColor(invoice)};";

        private Period Period = new(DateTime.Now, Enums.PeriodType.Month);

        private string presale_name = string.Empty;

        private Kpi? response;

        private async Task DownloadReport() => await response.Download(js, presale_name, Period, Localization);

        private async Task GenerateReport()
        {
            this.response = null;

            downloadButtonDisabled = true;

            if (string.IsNullOrEmpty(presale_name))
            {
                await GlobalMsgHandler.Show($"{Localization["NeedSelectPresaleMessageText"]}");
                return;
            }

            var response = await AppApi.GetKpiAsync(new KpiRequest
            {
                PresaleName = presale_name,
                Period = Period.Translate()
            }
            );

            if (response.ResultCase == KpiResponse.ResultOneofCase.Kpi)
            {
                this.response = response.Kpi;
                downloadButtonDisabled = false;
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
    }
}

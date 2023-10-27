using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages
{
    partial class Unpaid
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        private UnpaidProjects? response;
        private string presale_name = string.Empty;
        private bool is_main_project_include = false;

        private Period Period = new(new(DateTime.Now.Year, DateTime.Now.Month, 1), Enums.PeriodType.Month);

        private Dictionary<string, object> DownloadBtnAttrs { get; set; } = new() { { "disabled", "disabled" } };

        protected override async Task OnParametersSetAsync() => await UpdateData();

        private async Task UpdateData()
        {
            try
            {
                response = await AppApi.GetUnpaidProjectsAsync(new UnpaidRequest()
                {
                    IsMainProjectInclude = is_main_project_include,
                    PresaleName = presale_name,
                    Period = Period.Translate()
                });
                DownloadBtnAttrs = [];
            }
            catch (Exception e)
            {
                DownloadBtnAttrs = new() { { "disabled", "disabled" } };
                await GlobalMsgHandler.Show(e.Message);
            }
            StateHasChanged();
        }

        private async Task DownloadFile() => await response.Download(js, Localization);

        private async void OnModeChanged(object? obj)
        {
            is_main_project_include = obj == null ? is_main_project_include : (bool)obj;
            await UpdateData();
        }
    }
}

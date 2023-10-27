using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Pages
{
    public partial class Funnel
    {
        private static string SetCellColor(Project project) =>
            !project.Actions.Any(a => a.SalesFunnel) || project.FunnelStage == FunnelStage.None ? "--rz-danger" : "--rz-text-color";

        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        protected override async Task OnParametersSetAsync() => await UpdateData();

        private FunnelProjects? response;

        private RadzenDataGrid<Project> grid;

        private bool allRowsExpanded = false;

        private FunnelStage? _selectedStage = null;

        private DateTime? _selectedApprovalByTechDirectorAt = DateTime.MinValue;
        private DateTime? _selectedLastAction = DateTime.MinValue;

        private DateTime? _selectedDate = DateTime.MinValue;

        private void DropDown0Change(object args)
        {
            if (_selectedStage == FunnelStage.Any)
            {
                _selectedStage = null;
            }
        }

        private async Task ToggleRowsExpand()
        {
            allRowsExpanded = !allRowsExpanded;

            if (allRowsExpanded)
            {
                await grid.ExpandRows(grid.PagedView);
            }
            else
            {
                await grid.CollapseRows(grid.PagedView);
            }
        }

        private async Task UpdateData()
        {
            try
            {
                response = await AppApi.GetFunnelProjectsAsync(new Empty());
            }
            catch (Exception e)
            {
                await GlobalMsgHandler.Show(e.Message);
            }

            StateHasChanged();
        }
    }
}

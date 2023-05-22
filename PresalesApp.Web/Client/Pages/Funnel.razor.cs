using Blazorise.DataGrid;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Radzen.Blazor;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace PresalesApp.Web.Client.Pages
{
    public partial class Funnel
    {
        string SetCellColor(Project project) =>
            !project.Actions.Any(a => a.SalesFunnel) || project.FunnelStage == FunnelStage.None ? "--rz-danger" : "--rz-text-color";

        [CascadingParameter]
        public MessageSnackbar messageHandler { get; set; }

        protected override async Task OnParametersSetAsync() => await UpdateData();

        FunnelProjects? response;

        RadzenDataGrid<Project> grid;

        bool allRowsExpanded = false;

        FunnelStage? _selectedStage = null;

        DateTime? _selectedApprovalByTechDirectorAt = DateTime.MinValue;
        DateTime? _selectedLastAction = DateTime.MinValue;

        DateTime? _selectedDate = DateTime.MinValue;

        void DropDown0Change(object args)
        {
            if (_selectedStage == FunnelStage.Any)
            {
                _selectedStage = null;
            }
        }

        async Task ToggleRowsExpand()
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

        async Task UpdateData()
        {
            try
            {
                response = await AppApi.GetFunnelProjectsAsync(new Empty());
            }
            catch (Exception e)
            {
                await messageHandler.Show(e.Message);
            }

            StateHasChanged();
        }
    }
}

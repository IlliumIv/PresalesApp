using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Pages
{
    public partial class Funnel
    {
        private static string _SetCellColor(Project project) =>
            !project.Actions.Any(a => a.SalesFunnel) || project.FunnelStage == FunnelStage.None ? "--rz-danger" : "--rz-text-color";

        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        protected override async Task OnParametersSetAsync() => await _UpdateData();

        private FunnelProjects? _Response;

        private RadzenDataGrid<Project> _Grid;

        private bool _AllRowsExpanded = false;

        private FunnelStage? _selectedStage = null;

        private DateTime? _SelectedApprovalByTechDirectorAt = DateTime.MinValue;
        private DateTime? _SelectedLastAction = DateTime.MinValue;

        private DateTime? _SelectedDate = DateTime.MinValue;

        private void _DropDown0Change(object args)
        {
            if (_selectedStage == FunnelStage.Any)
            {
                _selectedStage = null;
            }
        }

        private async Task _ToggleRowsExpand()
        {
            _AllRowsExpanded = !_AllRowsExpanded;

            if (_AllRowsExpanded)
            {
                await _Grid.ExpandRows(_Grid.PagedView);
            }
            else
            {
                await _Grid.CollapseRows(_Grid.PagedView);
            }
        }

        private async Task _UpdateData()
        {
            try
            {
                _Response = await AppApi.GetFunnelProjectsAsync(new Empty());
            }
            catch (Exception e)
            {
                await GlobalMsgHandler.Show(e.Message);
            }

            StateHasChanged();
        }
    }
}

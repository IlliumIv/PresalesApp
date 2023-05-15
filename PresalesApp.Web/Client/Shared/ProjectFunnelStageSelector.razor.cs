using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Shared
{
    public partial class ProjectFunnelStageSelector
    {

        [CascadingParameter]
        public MessageSnackbar messageHandler { get; set; }

        [Parameter, EditorRequired]
        public Project Project { get; set; }

        [Parameter, EditorRequired]
        public RadzenDataGrid<Project> DataGrid { get; set; }

        [Parameter]
        public EventCallback<FunnelStage> OnChange { get; set; }

        bool _disabled = false;

        string _imgDisplay = $"display: none";

        FunnelStage _selectedStage;

        readonly IEnumerable<FunnelStage> _stages = Enum.GetValues(typeof(FunnelStage)).Cast<FunnelStage>().Where(i => i != FunnelStage.Any);

        protected override void OnParametersSet()
        {
            _selectedStage = Project.FunnelStage;
        }

        async Task DropDown0Change(object stage)
        {
            if ((FunnelStage)stage == FunnelStage.Any) return;

            var isExpanded = DataGrid.IsRowExpanded(Project);
            if (isExpanded) { await DataGrid.CollapseRows(new Project[] { Project }); }

            _disabled = true;
            _imgDisplay = $"display: initial";
            var message = "Stage updated sucessfully";
            var color = SnackbarColor.Success;

            try
            {
                var result = await BridgeApi.SetProjectFunnelStageAsync(new Bridge1C.NewProjectFunnelStage
                {
                    NewStage = (FunnelStage)stage,
                    ProjectNumber = Project.Number
                });

                if (!result.IsSuccess)
                    throw new Exception(result.ErrorMessage);
            }
            catch (Exception e)
            {
                message = e.Message;
                color = SnackbarColor.Danger;
                stage = Project.FunnelStage;
            }

            _imgDisplay = $"display: none";
            Project.FunnelStage = (FunnelStage)stage;
            _selectedStage = Project.FunnelStage;
            _disabled = false;

            await messageHandler.Show(message, color);

            if (isExpanded) { await DataGrid.ExpandRows(new Project[] { Project }); }
            await OnChange.InvokeAsync((FunnelStage)stage);
        }
    }
}

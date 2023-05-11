using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Shared
{
    public partial class ProjectFunnelStageSelector
    {

        [CascadingParameter]
        public MessageSnackbar messageHandler { get; set; }

        [Parameter]
        public Project Project { get; set; }

        bool _disabled = false;

        string _imgDisplay = $"display: none";

        FunnelStage _selectedStage;

        readonly IEnumerable<FunnelStage> _stages = Enum.GetValues(typeof(FunnelStage)).Cast<FunnelStage>();

        protected override void OnParametersSet()
        {
            _selectedStage = Project.FunnelStage;
        }

        async Task DropDown0Change(object stage)
        {
            if ((FunnelStage)stage == FunnelStage.Any) return;
            _disabled = true;
            _imgDisplay = $"display: initial";

            // Api
            try
            {
                var result = await BridgeApi.SetProjectFunnelStageAsync(new Bridge1C.NewProjectFunnelStage
                {
                    NewStage = (FunnelStage)stage,
                    ProjectNumber = Project.Number
                });

                if (!result.IsSuccess) throw new Exception(result.ErrorMessage);
            }
            catch (Exception e)
            {
                await messageHandler.Show(e.Message);
                stage = Project.FunnelStage;
            }

            _imgDisplay = $"display: none";
            Project.FunnelStage = (FunnelStage)stage;
            _selectedStage = Project.FunnelStage;
            _disabled = false;
        }
    }
}

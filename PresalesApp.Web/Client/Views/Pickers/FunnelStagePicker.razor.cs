using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Views.Pickers;

public partial class FunnelStagePicker
{

    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    [Parameter, EditorRequired]
    public Project Project { get; set; }

    [Parameter, EditorRequired]
    public RadzenDataGrid<Project> DataGrid { get; set; }

    [Parameter]
    public EventCallback<FunnelStage> OnChange { get; set; }

    private bool _Disabled = false;

    private string _ImgDisplay = $"display: none";

    private FunnelStage _SelectedStage;

    private readonly IEnumerable<FunnelStage> _Stages =
        Enum.GetValues<FunnelStage>().Cast<FunnelStage>().Where(i => i != FunnelStage.Any);

    protected override void OnParametersSet() => _SelectedStage = Project.FunnelStage;

    private async Task _DropDown0Change(object stage)
    {
        if ((FunnelStage)stage == FunnelStage.Any)
        {
            return;
        }

        var isExpanded = DataGrid.IsRowExpanded(Project);
        if (isExpanded) { await DataGrid.CollapseRows([Project]); }

        _Disabled = true;
        _ImgDisplay = $"display: initial";
        var message = "Stage updated sucessfully";
        var color = SnackbarColor.Success;

        try
        {
            var response = await BridgeApi.SetProjectFunnelStageAsync(new()
            {
                NewStage = (FunnelStage)stage,
                ProjectNumber = Project.Number
            });

            if(!response.IsSuccess)
            {
                throw new Exception(response.Error.Message);
            }
        }
        catch (Exception e)
        {
            message = e.Message;
            color = SnackbarColor.Danger;
            stage = Project.FunnelStage;
        }

        _ImgDisplay = $"display: none";
        Project.FunnelStage = (FunnelStage)stage;
        _SelectedStage = Project.FunnelStage;
        _Disabled = false;

        GlobalMsgHandler.Show(message, color);

        if (isExpanded) await DataGrid.ExpandRows([Project]);

        await OnChange.InvokeAsync((FunnelStage)stage);
    }
}

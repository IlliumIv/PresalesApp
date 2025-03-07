using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;
using Radzen;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Views.Pickers;

public partial class FunnelStagePicker
{
    #region Injections

    [Inject]
    private NotificationService _NotificationService { get; set; }

    #endregion

    #region Parameters

    [Parameter, EditorRequired]
    public Project Project { get; set; }

    [Parameter, EditorRequired]
    public RadzenDataGrid<Project> DataGrid { get; set; }

    [Parameter]
    public EventCallback<FunnelStage> OnChange { get; set; }

    #endregion

    #region Private

    #region Members

    private bool _Disabled = false;

    private string _ImgDisplay = $"display: none";

    private FunnelStage _SelectedStage;

    private readonly IEnumerable<FunnelStage> _Stages =
        Enum.GetValues<FunnelStage>().Cast<FunnelStage>().Where(i => i != FunnelStage.Any);

    #endregion

    #region Methods

    private async Task _DropDown0Change(object stage)
    {
        if ((FunnelStage)stage == FunnelStage.Any)
        {
            return;
        }

        var isExpanded = DataGrid.IsRowExpanded(Project);
        if (isExpanded)
        { await DataGrid.CollapseRows([Project]); }

        _Disabled = true;
        _ImgDisplay = $"display: initial";
        var message = "Stage updated sucessfully";
        var color = NotificationSeverity.Success;

        try
        {
            var response = await BridgeApi.SetProjectFunnelStageAsync(new()
            {
                NewStage = (FunnelStage)stage,
                ProjectNumber = Project.Number
            });

            if (!response.IsSuccess)
            {
                throw new Exception(response.Error.Message);
            }
        }
        catch (Exception e)
        {
            message = e.Message;
            color = NotificationSeverity.Error;
            stage = Project.FunnelStage;
        }

        _ImgDisplay = $"display: none";
        Project.FunnelStage = (FunnelStage)stage;
        _SelectedStage = Project.FunnelStage;
        _Disabled = false;

        _NotificationService.Notify(color, message);

        if (isExpanded)
            await DataGrid.ExpandRows([Project]);

        await OnChange.InvokeAsync((FunnelStage)stage);
    }

    #endregion

    #endregion

    protected override void OnParametersSet() => _SelectedStage = Project.FunnelStage;
}

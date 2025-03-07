using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;
using Radzen;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Pages;

public partial class Funnel
{
    private static string _SetCellColor(Project project)
        => !project.Actions.Any(a => a.SalesFunnel) || project.FunnelStage == FunnelStage.None
            ? "--rz-danger"
            : "--rz-text-color";

    [Inject]
    private NotificationService _NotificationService { get; set; }

    protected override async Task OnParametersSetAsync() => await _UpdateData();

    private GetProjectsResponse _Response;

    private RadzenDataGrid<Project> _Grid;

    private bool _AllRowsExpanded = false;

    private FunnelStage? _SelectedStage = null;

    private DateTime? _SelectedApprovalByTechDirectorAt = DateTime.MinValue;

    private DateTime? _SelectedLastAction = DateTime.MinValue;

    private DateTime? _SelectedDate = DateTime.MinValue;

    private void _DropDown0Change(object args)
    {
        if (_SelectedStage == FunnelStage.Any)
        {
            _SelectedStage = null;
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
            _Response = await PresalesAppApi.GetFunnelProjectsAsync(new());
        }
        catch (Exception e)
        {
            _NotificationService.Notify(NotificationSeverity.Error,
                e.Message);
        }

        StateHasChanged();
    }
}

using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;
using Radzen;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class PresalePicker
{
    [Inject]
    private NotificationService _NotificationService { get; set; }

    [Parameter]
    public EventCallback<string> OnSelectCallback { get; set; }

    [Parameter]
    public string SelectedPresale { get; set; } = string.Empty;

    private GetNamesResponse _PresalesResponse;

    private IEnumerable<string> _Names => _PresalesResponse?.Names ?? [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _PresalesResponse = await PresalesAppApi.GetNamesAsync(new());
        }
        catch (Exception e)
        {
            _NotificationService.Notify(NotificationSeverity.Error, e.Message);
        }
    }

    private void _OnPresaleChanged(string e)
    {
        SelectedPresale = e;
        OnSelectCallback.InvokeAsync(SelectedPresale);
    }
}

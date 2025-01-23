using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class PresalePicker
{
    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    [Parameter]
    public EventCallback<string> OnSelectCallback { get; set; }

    [Parameter]
    public string SelectedPresale { get; set; } = string.Empty;

    private GetNamesResponse? _PresalesResponse;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _PresalesResponse = await PresalesAppApi.GetNamesAsync(new());
        }
        catch (Exception e)
        {
            GlobalMsgHandler.Show(e.Message);
        }
    }

    private void _OnPresaleChanged(ChangeEventArgs e)
    {
        SelectedPresale = e?.Value?.ToString() ?? string.Empty;
        OnSelectCallback.InvokeAsync(SelectedPresale);
    }
}

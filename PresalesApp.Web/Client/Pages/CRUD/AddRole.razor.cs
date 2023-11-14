using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Shared;
using Radzen;

namespace PresalesApp.Web.Client.Pages.CRUD;

public partial class AddRole
{
    [Inject]
    protected DialogService DialogService { get; set; }

    [Inject]
    protected Api.ApiClient ApiClient { get; set; }

    protected override void OnInitialized() => Role = new() { Name = "" };

    protected bool IsErrorVisible;
    protected Role Role;

    protected async Task FormSubmit()
    {
        try
        {
            _ = await ApiClient.CreateRoleAsync(Role);
            DialogService.Close(Role);
        }
        catch(Exception)
        {
            IsErrorVisible = true;
        }
    }

    protected void CancelButtonClick(MouseEventArgs args) => DialogService.Close(null);
}
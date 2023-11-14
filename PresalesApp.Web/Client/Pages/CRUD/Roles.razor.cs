using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Shared;
using Radzen;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Pages.CRUD;

public partial class Roles
{
    [Inject]
    protected NotificationService NotificationService { get; set; }

    [Inject]
    protected DialogService DialogService { get; set; }

    [Inject]
    protected Api.ApiClient ApiClient { get; set; }

    protected IEnumerable<Role> _Roles;

    protected RadzenDataGrid<Role> grid0;

    protected int count;

    protected string search = "";

    protected async Task Search(ChangeEventArgs args)
    {
        search = $"{args.Value}";

        await grid0.GoToPage(0);

        await grid0.Reload();
    }

    protected async Task Grid0LoadData(LoadDataArgs args)
    {
        try
        {
            var result = await ApiClient.GetRolesAsync(new());

            _Roles = result.Roles_.AsODataEnumerable();
            count = result.Roles_.Count;
        }
        catch(System.Exception)
        {
            NotificationService.Notify(new NotificationMessage() { Severity = NotificationSeverity.Error, Summary = $"Error", Detail = $"Unable to load AspNetRoles" });
        }
    }

    protected async Task AddButtonClick(MouseEventArgs args)
    {
        await DialogService.OpenAsync<AddRole>("Add AspNetRole", null);
        await grid0.Reload();
    }

    protected async Task EditRow(Role args) =>
        // await DialogService.OpenAsync<EditAspNetRole>("Edit AspNetRole", new Dictionary<string, object> { { "Id", args.Id } });
        await grid0.Reload();

    protected async Task GridDeleteButtonClick(MouseEventArgs args, Role role)
    {
        try
        {
            if(await DialogService.Confirm("Are you sure you want to delete this record?") == true)
            {
                var deleteResult = await ApiClient.DeleteRoleAsync(role);

                if(deleteResult != null)
                {
                    await grid0.Reload();
                }
            }
        }
        catch(Exception)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = $"Error",
                Detail = $"Unable to delete AspNetRole"
            });
        }
    }
}
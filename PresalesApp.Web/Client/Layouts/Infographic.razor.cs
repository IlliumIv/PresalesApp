using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace PresalesApp.Web.Client.Layouts;

partial class Infographic
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthenticationState { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var state = await AuthenticationState;
        if (state.User.Identity is null || !state.User.Identity.IsAuthenticated)
        {
            Navigation.NavigateTo(Navigation.BaseUri, true);
        }
    }
}

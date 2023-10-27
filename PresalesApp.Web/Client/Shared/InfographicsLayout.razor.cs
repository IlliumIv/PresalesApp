using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Shared
{
    partial class InfographicsLayout
    {
        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationState { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            AuthenticationState state = await AuthenticationState;
            if (state.User.Identity is null || !state.User.Identity.IsAuthenticated)
            {
                Navigation.NavigateTo(Navigation.BaseUri, true);
            }
        }
    }
}

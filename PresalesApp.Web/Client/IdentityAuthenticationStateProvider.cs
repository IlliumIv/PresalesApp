using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace PresalesApp.Web.Client
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public IdentityAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("token");

            if (!string.IsNullOrEmpty(token))
            {
                var authUser = new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, "admin")
                }, "jwt"));

                return new AuthenticationState(authUser);
            }

            return new AuthenticationState(new ClaimsPrincipal());
        }

        public void MarkUserAsAuthenticated()
        {
            var authState = GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(authState);
        }
    }
}

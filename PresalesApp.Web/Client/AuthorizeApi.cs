using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace PresalesApp.Web.Client
{
    public class AuthorizeApi
    {
        private readonly IdentityAuthenticationStateProvider _stateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthorizeApi(AuthenticationStateProvider stateProvider, ILocalStorageService storage)
        {
            _stateProvider = (IdentityAuthenticationStateProvider)stateProvider;
            _localStorage = storage;
        }

        public async Task<bool> Login(string username, string password)
        {
            var token = "1231d12de1e12e12ds2ds12";
            await _localStorage.SetItemAsync("token", token);

            _stateProvider.MarkUserAsAuthenticated();
            return true;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
        }
    }
}

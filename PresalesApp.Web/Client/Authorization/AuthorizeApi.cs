using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PresalesApp.Web.Shared;
using AppApi = PresalesApp.Web.Shared.Api.ApiClient;

namespace PresalesApp.Web.Client.Authorization
{
    public class AuthorizeApi
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AppApi _apiClient;
        private readonly IdentityAuthenticationStateProvider _stateProvider;

        public AuthorizeApi(
            AuthenticationStateProvider stateProvider,
            ILocalStorageService storage,
            AppApi presalesAppApiClient)
        {
            _stateProvider = (IdentityAuthenticationStateProvider) stateProvider;
            _localStorage = storage;
            _apiClient = presalesAppApiClient;
        }

        public async Task<bool> TryRegister(RegisterRequest registerRequest)
        {
            var responce = await _apiClient.RegisterAsync(registerRequest);
            return await ProceedLoginResponceAsync(responce);
        }

        public async Task<bool> TryLogin(LoginRequest loginRequest)
        {
            var responce = await _apiClient.LoginAsync(loginRequest);
            return await ProceedLoginResponceAsync(responce);
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("token");
            _stateProvider.MarkLogouted();
        }

        private async Task<bool> ProceedLoginResponceAsync(LoginResponse loginResponse)
        {
            if (loginResponse.ResultCase == LoginResponse.ResultOneofCase.UserInfo)
            {
                await _localStorage.SetItemAsStringAsync("token", loginResponse.UserInfo.Token);
                _stateProvider.MarkUserAsAuthenticated(loginResponse.UserInfo);
                return true;
            }
            if (loginResponse.ResultCase == LoginResponse.ResultOneofCase.Error)
            {
                throw new Exception(loginResponse.Error.Message);
            }

            return false;
        }
    }
}

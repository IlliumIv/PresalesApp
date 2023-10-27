using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PresalesApp.Web.Shared;
using AppApi = PresalesApp.Web.Shared.Api.ApiClient;

namespace PresalesApp.Web.Client.Authorization
{
    public class AuthorizeApi(
        AuthenticationStateProvider stateProvider,
        ILocalStorageService storage,
        AppApi presalesAppApiClient)
    {
        private readonly ILocalStorageService _localStorage = storage;
        private readonly AppApi _apiClient = presalesAppApiClient;
        private readonly IdentityAuthenticationStateProvider _stateProvider = (IdentityAuthenticationStateProvider)stateProvider;

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

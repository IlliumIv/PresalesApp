﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Authorization
{
    public class AuthorizeApi
    {
        private readonly ILocalStorageService _localStorage;
        private readonly PresalesAppApi.PresalesAppApiClient _apiClient;
        private readonly IdentityAuthenticationStateProvider _stateProvider;

        public AuthorizeApi(
            AuthenticationStateProvider stateProvider,
            ILocalStorageService storage,
            PresalesAppApi.PresalesAppApiClient presalesAppApiClient)
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
            await _localStorage.RemoveItemAsync(Settings.Default.StorageTokenKey);
            _stateProvider.MarkLogouted();
        }

        private async Task<bool> ProceedLoginResponceAsync(LoginResponse loginResponse)
        {
            if (loginResponse.ResultCase == LoginResponse.ResultOneofCase.UserInfo)
            {
                await _localStorage.SetItemAsStringAsync(Settings.Default.StorageTokenKey, loginResponse.UserInfo.Token);
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
using Blazored.LocalStorage;
using PresalesApp.Web.Shared;
using ApiClient = PresalesApp.Web.Shared.Api.ApiClient;

namespace PresalesApp.Web.Client.Services.Authorization;

public class AuthorizationService(
    IdentityAuthenticationStateProvider provider,
    ILocalStorageService storage,
    ApiClient client)
{
    private readonly ILocalStorageService _LocalStorage = storage;
    private readonly ApiClient _ApiClient = client;
    private readonly IdentityAuthenticationStateProvider _StateProvider = provider;

    public async Task<bool> TryRegister(RegisterRequest registerRequest)
    {
        var responce = await _ApiClient.RegisterAsync(registerRequest);
        return await _ProceedLoginResponceAsync(responce);
    }

    public async Task<bool> TryLogin(LoginRequest loginRequest)
    {
        var responce = await _ApiClient.LoginAsync(loginRequest);
        return await _ProceedLoginResponceAsync(responce);
    }

    public async Task Logout()
    {
        await _LocalStorage.RemoveItemAsync("token");
        _StateProvider.MarkLogouted();
    }

    private async Task<bool> _ProceedLoginResponceAsync(LoginResponse loginResponse)
    {
        if(loginResponse.ResultCase == LoginResponse.ResultOneofCase.UserInfo)
        {
            await _LocalStorage.SetItemAsStringAsync("token", loginResponse.UserInfo.Token);
            _StateProvider.MarkUserAsAuthenticated(loginResponse.UserInfo);
            return true;
        }
        return loginResponse.ResultCase == LoginResponse.ResultOneofCase.Error ? throw new Exception(loginResponse.Error.Message) : false;
    }
}

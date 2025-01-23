using Blazored.LocalStorage;
using PresalesApp.Authorization;
using AuthorizationClient = PresalesApp.Authorization
    .AuthorizationService.AuthorizationServiceClient;

namespace PresalesApp.Web.Client.Services.Authorization;

public class AuthorizationService(IdentityAuthenticationStateProvider provider,
    ILocalStorageService storage, AuthorizationClient client)
{
    private readonly IdentityAuthenticationStateProvider _StateProvider = provider;

    private readonly ILocalStorageService _LocalStorage = storage;

    private readonly AuthorizationClient _ApiClient = client;

    public async Task<bool> TryRegister(RegisterRequest registerRequest)
    {
        var responce = await _ApiClient.RegisterAsync(registerRequest);
        return await _ProceedLoginResponceAsync(responce);
    }

    public async Task<bool> TryLogin(LoginRequest loginRequest)
    {
        var some = _ApiClient.LoginAsync(loginRequest);

        var responce = await some;
        return await _ProceedLoginResponceAsync(responce);
    }

    public async Task Logout()
    {
        await _LocalStorage.RemoveItemAsync("token");
        _StateProvider.MarkLogouted();
    }

    private async Task<bool> _ProceedLoginResponceAsync(LoginResponse loginResponse)
    {
        if(loginResponse.KindCase == LoginResponse.KindOneofCase.User)
        {
            await _LocalStorage.SetItemAsStringAsync("token", loginResponse.User.Token);
            _StateProvider.MarkUserAsAuthenticated(loginResponse.User);
            return true;
        }

        return loginResponse.KindCase == LoginResponse.KindOneofCase.Error
            ? throw new Exception(loginResponse.Error.Message)
            : false;
    }
}

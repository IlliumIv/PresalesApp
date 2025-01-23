using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using PresalesApp.Authorization;

namespace PresalesApp.Web.Client.Services.Authorization;

public class IdentityAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly ILocalStorageService _LocalStorage;

    private static readonly Task<AuthenticationState> _DefaultUnauthenticatedTask
        = Task.FromResult(new AuthenticationState(new(new ClaimsIdentity())));

    private readonly Task<AuthenticationState> _AuthenticationStateTask
        = _DefaultUnauthenticatedTask;

    private async Task<AuthenticationState> _GetStateAsync()
    {
        var token = await _LocalStorage.GetItemAsStringAsync("token");
        var authState = JwtTokenExtensions.GetStateFromJwt(token);

        return authState;
    }
    public IdentityAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _LocalStorage = localStorage;
        _AuthenticationStateTask = _GetStateAsync();
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => _AuthenticationStateTask;

    public AuthenticationState MarkUserAsAuthenticated(User user)
    {
        var authState = JwtTokenExtensions.GetStateFromJwt(user.Token);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
        return authState;
    }

    public void MarkLogouted() => NotifyAuthenticationStateChanged(
        Task.FromResult(new AuthenticationState(new(new ClaimsIdentity()))));
}

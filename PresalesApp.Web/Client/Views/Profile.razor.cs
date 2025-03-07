using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PresalesApp.Web.Client.Services.Authorization;
using PresalesApp.Web.Client.Views.Autorization;
using Radzen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PresalesApp.Web.Client.Views;

partial class Profile
{
    [Inject]
    private IdentityAuthenticationStateProvider _AuthStateProvider { get; set; }

    [Inject]
    private AuthorizationService _AutorizeApi { get; set; }

    [Inject]
    private DialogService _DialogService { get; set; }

    private string _UserName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authTask = await _AuthStateProvider.GetAuthenticationStateAsync();
        _UserName = (authTask.User.Identity as ClaimsIdentity)?
            .FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;

        _AuthStateProvider.AuthenticationStateChanged
            += _AuthStateChanged;
    }

    private void _AuthStateChanged(Task<AuthenticationState> authTask)
    {
        _UserName = (authTask.Result.User.Identity as ClaimsIdentity)?
            .FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;
        StateHasChanged();
    }

    private async Task _Logout() => await _AutorizeApi.Logout();

    private async Task _OpenRegisterForm()
        => await _DialogService.OpenAsync<Register>(Localization["RegisterModalButtonText"],
            null, Register.DefaultDialogOptions);

    private async Task _OpenLoginForm()
        => await _DialogService.OpenAsync<Login>(Localization["LoginButtonModalText"],
            null, Login.DefaultDialogOptions);
}

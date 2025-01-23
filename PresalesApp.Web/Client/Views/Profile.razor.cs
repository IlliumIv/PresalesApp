using Blazorise;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Services.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PresalesApp.Web.Client.Views;

partial class Profile
{
    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    [Inject]
    protected IdentityAuthenticationStateProvider AuthStateProvider { get; set; }

    private string _UserName => (AuthStateProvider.GetAuthenticationStateAsync().Result
        .User?.Identity as ClaimsIdentity)?
        .FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;

    private string _DesireUserName { get; set; } = string.Empty;

    private Modal _LoginModal;
    private Modal _RegisterModal;

    private bool _LoginDisabled = false;
    private bool _RegisterDisabled = false;

    private string _Login;
    private string _Password;

    private async Task _TryRegister()
    {
        _RegisterDisabled = true;
        try
        {
            if (await _autorizeApi.TryRegister(new()
            {
                LoginRequest = new()
                {
                    Login = _Login,
                    Password = _Password
                },
                User = new()
                {
                    Name = _DesireUserName
                }
            }))
            {
                await _RegisterModal.Hide();
            }
        }
        catch (Exception e)
        {
            GlobalMsgHandler.Show(e.Message);
        }

        _RegisterDisabled = false;
    }

    private async Task _TryLogin()
    {
        _LoginDisabled = true;
        try
        {
            if (await _autorizeApi.TryLogin(new()
            {
                Login = _Login,
                Password = _Password
            }))
            {
                await _LoginModal.Hide();
            }
        }
        catch (Exception e)
        {
            GlobalMsgHandler.Show(e.Message);
        }

        _LoginDisabled = false;
    }

    private async Task _Logout() => await _autorizeApi.Logout();
}

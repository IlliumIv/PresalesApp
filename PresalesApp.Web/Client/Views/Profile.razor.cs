using Blazorise;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Services.Authorization;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Views
{
    partial class Profile
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        private static UserProfile GetProfile() => IdentityAuthenticationStateProvider.Profile;

        private Modal _loginModal;
        private Modal _registerModal;

        private bool _loginDisabled = false;
        private bool _registerDisabled = false;

        private string _login;
        private string _password;

        private async Task Register()
        {
            _registerDisabled = true;
            try
            {
                if (await _autorizeApi.TryRegister(new RegisterRequest
                {
                    LoginRequest = new LoginRequest
                    {
                        Login = _login,
                        Password = _password
                    },
                    Profile = GetProfile()
                }))
                {
                    await _registerModal.Hide();
                }
            }
            catch (Exception e)
            {
                GlobalMsgHandler.Show(e.Message);
            }
            _registerDisabled = false;
        }

        private async Task Login()
        {
            _loginDisabled = true;
            try
            {
                if (await _autorizeApi.TryLogin(new LoginRequest
                {
                    Login = _login,
                    Password = _password
                }))
                {
                    await _loginModal.Hide();
                }
            }
            catch (Exception e)
            {
                GlobalMsgHandler.Show(e.Message);
            }
            _loginDisabled = false;
        }

        private async Task Logout()
        {
            await _autorizeApi.Logout();
        }
    }
}

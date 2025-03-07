using Microsoft.AspNetCore.Components;
using PresalesApp.Authorization;
using Radzen;

namespace PresalesApp.Web.Client.Views.Autorization;

public partial class Register
{
    public static readonly DialogOptions DefaultDialogOptions = new()
    {
        CloseDialogOnOverlayClick = true,
        Resizable = true,
        Width = "30vw"
    };

    [Inject]
    private Services.Authorization.AuthorizationService _AutorizeApi { get; set; }

    [Inject]
    private NotificationService _NotificationService { get; set; }

    [Inject]
    private DialogService _DialogService { get; set; }

    private class _RegisterModel
    {
        public User User { get; set; } = new();

        public LoginRequest LoginRequest { get; set; } = new();
    }

    private readonly _RegisterModel _FormModel = new();

    bool _Busy = false;

    bool _ShowPassword = false;

    private void _OnProfileNameChange(string value) => _FormModel.User.Name = value;

    private void _OnLoginChange(string value) => _FormModel.LoginRequest.Login = value;

    private void _OnPasswordChange(string value) => _FormModel.LoginRequest.Password = value;

    private async Task _OnSubmitAsync() => await _TryRegisterAsync();

    private void _OnInvalidSubmit()
        => _NotificationService.Notify(NotificationSeverity.Error,
            Localization["FormValidationErrorText"]);

    private async Task _TryRegisterAsync()
    {
        try
        {
            _Busy = true;
            if (await _AutorizeApi.TryRegister(new()
            {
                LoginRequest = _FormModel.LoginRequest,
                User = _FormModel.User
            }))
            {
                _DialogService.Close();
            }
        }
        catch (Exception e)
        {
            _NotificationService.Notify(NotificationSeverity.Error,
                e.Message);
        }
        finally
        {
            _Busy = false;
        }
    }

    private async Task _OpenLoginFormAsync()
    {
        _DialogService.Close();
        await _DialogService.OpenAsync<Login>(Localization["LoginButtonModalText"],
            null, Login.DefaultDialogOptions);
    }
}

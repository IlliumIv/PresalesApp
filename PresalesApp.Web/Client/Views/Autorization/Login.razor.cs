using Microsoft.AspNetCore.Components;
using Radzen;
using PresalesApp.Authorization;

namespace PresalesApp.Web.Client.Views.Autorization;

public partial class Login
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

    private readonly LoginRequest _Model = new();

    bool _Busy = false;

    bool _ShowPassword = false;

    private void _OnLoginChange(string value) => _Model.Login = value;

    private void _OnPasswordChange(string value) => _Model.Password = value;

    private async Task _OnSubmitAsync() => await _TryLoginAsync();

    private void _OnInvalidSubmit()
        => _NotificationService.Notify(NotificationSeverity.Error,
            Localization["FormValidationErrorText"]);

    private async Task _TryLoginAsync()
    {
        try
        {
            _Busy = true;
            if (await _AutorizeApi.TryLogin(_Model))
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

    private async Task _OpenRegisterFormAsync()
    {
        _DialogService.Close();
        await _DialogService.OpenAsync<Register>(Localization["RegisterModalButtonText"],
            null, Register.DefaultDialogOptions);
    }
}

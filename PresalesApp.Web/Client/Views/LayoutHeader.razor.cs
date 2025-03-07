using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace PresalesApp.Web.Client.Views;

public partial class LayoutHeader
{
    [Inject]
    private DialogService _DialogService {  get; set; }

    [Inject]
    private IJSRuntime _JsRuntime { get; set; }

    private async Task _OpenAppSettings()
        => await _DialogService.OpenAsync<AppSettings>(Localization["SettingsWindowTitle"],
            null, AppSettings.DefaultDialogOptions);
}

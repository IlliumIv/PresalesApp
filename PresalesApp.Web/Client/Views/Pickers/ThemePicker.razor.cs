using Microsoft.AspNetCore.Components;
using Radzen;

namespace PresalesApp.Web.Client.Views.Pickers;

public partial class ThemePicker
{
    [Inject]
    private ThemeService _ThemeService { get; set; }

    void _ChangeTheme(string value) => _ThemeService.SetTheme(value);
}
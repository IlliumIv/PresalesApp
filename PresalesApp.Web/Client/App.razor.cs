using Microsoft.AspNetCore.Components;
using Radzen;
namespace PresalesApp.Web.Client;

public partial class App
{
    [CascadingParameter]
    private Microsoft.AspNetCore.Http.HttpContext _HttpContext { get; set; }

    [Inject]
    private ThemeService _ThemeService { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (_HttpContext != null)
        {
            var theme = _HttpContext.Request.Cookies["PresalesAppTheme"];

            if (!string.IsNullOrEmpty(theme))
            {
                _ThemeService.SetTheme(theme, false);
            }
        }
    }
}

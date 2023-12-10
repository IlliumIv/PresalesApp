using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Views;

partial class FullScreenWarning
{
    [Parameter]
    public string Message { get; set; } = "Нужно проветрить!";

    [Parameter]
    public bool IsGlitched { get; set; } = false;

    private string _Display = "none";

    private TimeSpan _TimeLeft = new(0, 10, 0);
}

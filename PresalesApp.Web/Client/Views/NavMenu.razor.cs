namespace PresalesApp.Web.Client.Views
{
    partial class NavMenu
    {
        private bool collapseNavMenu = false;
        private string? GetNavMenuCssClass() => collapseNavMenu ? "collapse" : null;
        private string? GetCollapseIconCssClass() => collapseNavMenu ? "oi-chevron-right" : "oi-chevron-left";
        private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;
    }
}

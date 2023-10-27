namespace PresalesApp.Web.Client.Shared
{
    partial class NavMenu
    {
        private bool collapseNavMenu = true;
        private string? GetNavMenuCssClass() => collapseNavMenu ? "collapse" : null;
        private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;
    }
}

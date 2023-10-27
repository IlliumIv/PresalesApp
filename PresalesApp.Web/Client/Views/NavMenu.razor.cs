namespace PresalesApp.Web.Client.Views
{
    partial class NavMenu
    {
        private bool collapseNavMenu = true;
        private string? GetNavMenuCssClass() => collapseNavMenu ? "collapse" : null;
        private void ToggleNavMenu() => collapseNavMenu = !collapseNavMenu;
    }
}

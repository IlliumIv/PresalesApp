namespace PresalesApp.Web.Client.Views;

partial class NavMenu
{
    private bool _CollapseNavMenu { get; set; } = false;

    private string? _GetNavMenuCssClass() => _CollapseNavMenu ? "collapse" : null;

    private string? _GetCollapseIconCssClass() => _CollapseNavMenu
        ? "oi-chevron-right"
        : "oi-chevron-left";

    private void _ToggleNavMenu() => _CollapseNavMenu = !_CollapseNavMenu;
}

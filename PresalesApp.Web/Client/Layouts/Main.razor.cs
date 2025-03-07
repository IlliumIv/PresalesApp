using Microsoft.AspNetCore.Components;
using Radzen;

namespace PresalesApp.Web.Client.Layouts;

public partial class Main
{
    private bool _SidebarExpanded = true;

    void _SidebarToggleClick()
    {
        _SidebarExpanded = !_SidebarExpanded;
    }
}

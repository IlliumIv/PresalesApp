using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class DepartmentPicker
{
    [Parameter]
    public EventCallback<Department> OnSelectCallback { get; set; }

    [Parameter]
    public Department Department { get; set; } = Department.Any;

    private void _OnDepartmentChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<Department>(e?.Value?.ToString(), out var _d))
        {
            Department = _d;
            OnSelectCallback.InvokeAsync(Department);
        }
    }
}

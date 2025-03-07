using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class DepartmentPicker
{
    [Parameter]
    public EventCallback<Department> OnSelectCallback { get; set; }

    [Parameter]
    public Department Department { get; set; } = Department.Any;

    private readonly IEnumerable<Department> _Departments = Enum.GetValues<Department>();

    private void _OnDepartmentChanged(Department department)
    {
        Department = department;
        OnSelectCallback.InvokeAsync(Department);
    }
}

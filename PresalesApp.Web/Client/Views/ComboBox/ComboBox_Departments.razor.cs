using Microsoft.AspNetCore.Components;
using Department = PresalesApp.Web.Shared.Department;

namespace PresalesApp.Web.Client.Views.ComboBox
{
    partial class ComboBox_Departments
    {
        [Parameter]
        public EventCallback<Department> OnSelectCallback { get; set; }

        [Parameter]
        public Department Department { get; set; } = Department.Any;

        private void OnDepartmentChanged(object? obj)
        {
            if (Enum.TryParse<Department>(obj?.ToString(), out var _d))
            {
                Department = _d;
                OnSelectCallback.InvokeAsync(Department);
            }
        }
    }
}

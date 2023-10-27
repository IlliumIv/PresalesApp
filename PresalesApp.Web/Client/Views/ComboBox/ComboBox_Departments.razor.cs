using Department = PresalesApp.Web.Shared.Department;
using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Views.ComboBox
{
    partial class ComboBox_Departments
    {
        [Parameter]
        public EventCallback<Department> OnSelectCallback { get; set; }

        private Department department = Department.Any;

        private void OnDepartmentChanged(object? obj)
        {
            if (Enum.TryParse<Department>(obj?.ToString(), out department))
                OnSelectCallback.InvokeAsync(department);
        }
    }
}

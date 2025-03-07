using Microsoft.AspNetCore.Components;
using Radzen;
using System.Reflection;

namespace PresalesApp.Web.Client.Views.QuickGridExtension;

public partial class ColumnsSelector<TValue>
{
    public static readonly DialogOptions DefaultDialogOptions = new()
    {
        CloseDialogOnOverlayClick = true,
        Resizable = true,
        Width = "20vw"
    };

    [Parameter]
    public IEnumerable<string>? Values { get; set; } = [];

    [Parameter]
    public EventCallback OnSelectCallback { get; set; }

    private static PropertyInfo[] _GetProperties()
        => typeof(TValue).GetProperties(BindingFlags.Instance | BindingFlags.Public);

    private async Task _OnChangeAsync(IEnumerable<string>? values)
        => await OnSelectCallback.InvokeAsync(values?.ToArray() ?? []);
}

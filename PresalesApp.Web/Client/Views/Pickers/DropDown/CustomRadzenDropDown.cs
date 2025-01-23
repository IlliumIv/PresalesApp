using Radzen.Blazor;

namespace PresalesApp.Web.Client.Views.Pickers.DropDown;

public class CustomRadzenDropDown<T> : RadzenDropDown<T>
{
    public async Task CustomOpenPopup() => await OpenPopup();

    public void SetSelectedIndex(int index) => selectedIndex = index;
}

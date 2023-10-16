using Radzen.Blazor;

namespace PresalesApp.Web.Client.Shared.DropDown
{
    public class CustomRadzenDropDown<T> : RadzenDropDown<T>
    {
        public async Task CustomOpenPopup() => await OpenPopup();
    }
}

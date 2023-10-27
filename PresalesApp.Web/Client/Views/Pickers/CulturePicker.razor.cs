using System.Globalization;

namespace PresalesApp.Web.Client.Views.Pickers
{
    partial class CulturePicker
    {
        private readonly CultureInfo[] _supportedCultures =
        [
            new CultureInfo("ru-RU"),
            new CultureInfo("en-US")
        ];

        private void OnChanged(object? obj)
        {
            var newCulture = new CultureInfo(obj?.ToString() ?? "ru-RU");
            Storage.SetItemAsString("i18nextLng", newCulture.Name);
            Navigation.NavigateTo(Navigation.Uri, true);
        }
    }
}
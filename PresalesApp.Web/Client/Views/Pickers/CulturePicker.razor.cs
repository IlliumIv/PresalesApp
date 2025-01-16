using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views.Pickers.DropDown;
using System.Globalization;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class CulturePicker
{
    static readonly CultureInfo _Russian = new("ru-RU");
    static readonly CultureInfo _English = new("en-US");

    private readonly List<DropDownItem<CultureInfo>> _SupportedCultures =
        [
            new(_Russian, _Russian.NativeName.ToUpperFirstLetterString()),
            new(_English, _English.NativeName.ToUpperFirstLetterString())
        ];

    private DropDownItem<CultureInfo>? _Item = new(CultureInfo.CurrentCulture,
        CultureInfo.CurrentCulture.NativeName.ToUpperFirstLetterString());

    protected void SelectedItemChanged(object item)
    {
        Storage.SetItemAsString("i18nextLng", _Item?.Value.Name ?? "ru-RU");
        Navigation.NavigateTo(Navigation.Uri, true);
    }
}
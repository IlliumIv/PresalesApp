using Radzen;

namespace PresalesApp.Web.Client.Views;

public partial class AppSettings
{
    public static readonly DialogOptions DefaultDialogOptions = new()
    {
        CloseDialogOnOverlayClick = true,
        Resizable = true
    };
}

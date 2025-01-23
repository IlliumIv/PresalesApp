using Blazorise.Snackbar;

namespace PresalesApp.Web.Client.Views;

// TODO: Use Radzen Dialog instead or smt similar.
partial class MessageSnackbar
{
    private SnackbarStack _SnackbarStack;

    public async void Show(string message, SnackbarColor color = SnackbarColor.Danger,
                           double interval = 5000)
        => await _SnackbarStack.PushAsync(message, color,
            options => options.IntervalBeforeClose = interval);
}

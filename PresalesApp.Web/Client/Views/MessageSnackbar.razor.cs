using Blazorise.Snackbar;

namespace PresalesApp.Web.Client.Views
{
    partial class MessageSnackbar
    {
        private SnackbarStack _snackbarStack;

        public async void Show(string message, SnackbarColor color = SnackbarColor.Danger, double interval = 5000) =>
            await _snackbarStack.PushAsync(message, color, options => { options.IntervalBeforeClose = interval; });
    }
}

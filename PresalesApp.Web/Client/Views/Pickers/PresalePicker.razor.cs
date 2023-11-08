using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Views.Pickers
{
    partial class PresalePicker
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        [Parameter]
        public EventCallback<string> OnSelectCallback { get; set; }

        [Parameter]
        public string SelectedPresale { get; set; } = string.Empty;

        private NamesResponse? presales;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                presales = await AppApi.GetNamesAsync(new Empty());
            }
            catch (Exception e)
            {
                await GlobalMsgHandler.Show(e.Message);
            }
        }

        private void OnPresaleChanged(ChangeEventArgs e)
        {
            SelectedPresale = e?.Value?.ToString() ?? string.Empty;
            OnSelectCallback.InvokeAsync(SelectedPresale);
        }
    }
}

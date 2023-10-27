using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Views.ComboBox
{
    partial class ComboBox_PresalesNames
    {
        [Parameter]
        public EventCallback<string> OnSelectCallback { get; set; }

        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        private NamesResponse? presales;
        private string selectedName = string.Empty;

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

        private void OnPresaleChanged(object? obj)
        {
            selectedName = obj?.ToString() ?? string.Empty;
            OnSelectCallback.InvokeAsync(selectedName);
        }
    }
}

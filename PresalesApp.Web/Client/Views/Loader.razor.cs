using Microsoft.AspNetCore.Components;

namespace PresalesApp.Web.Client.Views
{
    partial class Loader
    {
        [Parameter]
        public int Width { get; set; } = 60;

        [Parameter]
        public int Height { get; set; } = 60;

        [Parameter]
        public string Style { get; set; }

        private string GetStyle() => $"width: {Width}px; height: {Height}px";
    }
}

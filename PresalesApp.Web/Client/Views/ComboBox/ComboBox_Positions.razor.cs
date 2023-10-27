using Microsoft.AspNetCore.Components;
using Position = PresalesApp.Web.Shared.Position;

namespace PresalesApp.Web.Client.Views.ComboBox
{
    partial class ComboBox_Positions
    {
        [Parameter]
        public EventCallback<Position> OnSelectCallback { get; set; }

        private Position position = Position.Any;

        private void OnPositionChanged(object? obj)
        {
            if (Enum.TryParse(obj?.ToString(), out position))
                OnSelectCallback.InvokeAsync(position);
        }
    }
}

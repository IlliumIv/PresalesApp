using Microsoft.AspNetCore.Components;
using Position = PresalesApp.Web.Shared.Position;

namespace PresalesApp.Web.Client.Views.ComboBox
{
    partial class ComboBox_Positions
    {
        [Parameter]
        public EventCallback<Position> OnSelectCallback { get; set; }

        [Parameter]
        public Position Position { get; set; } = Position.Any;

        private void OnPositionChanged(object? obj)
        {
            if (Enum.TryParse<Position>(obj?.ToString(), out var _p))
            {
                Position = _p;
                OnSelectCallback.InvokeAsync(Position);
            }
        }
    }
}

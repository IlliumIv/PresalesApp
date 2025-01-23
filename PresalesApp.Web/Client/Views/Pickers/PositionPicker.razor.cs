using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class PositionPicker
{
    [Parameter]
    public EventCallback<Position> OnSelectCallback { get; set; }

    [Parameter]
    public Position Position { get; set; } = Position.Any;

    private void _OnPositionChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<Position>(e?.Value?.ToString(), out var _p))
        {
            Position = _p;
            OnSelectCallback.InvokeAsync(Position);
        }
    }
}

using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class PositionPicker
{
    [Parameter]
    public EventCallback<Position> OnSelectCallback { get; set; }

    [Parameter]
    public Position Position { get; set; } = Position.Any;

    private readonly IEnumerable<Position> _Positions = Enum.GetValues<Position>();

    private void _OnPositionChanged(Position position)
    {
        Position = position;
        OnSelectCallback.InvokeAsync(Position);
    }
}

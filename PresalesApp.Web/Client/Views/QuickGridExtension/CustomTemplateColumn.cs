using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Rendering;

namespace PresalesApp.Web.Client.Views.QuickGridExtension;

public class CustomTemplateColumn<TGridItem> : CustomColumnBase<TGridItem>
{
    private static readonly RenderFragment<TGridItem> EmptyChildContent = _ => builder => { };

    /// <summary>
    /// Specifies the content to be rendered for each row in the table.
    /// </summary>
    [Parameter] public RenderFragment<TGridItem> ChildContent { get; set; } = EmptyChildContent;

    /// <inheritdoc/>
    [Parameter] public override GridSort<TGridItem>? SortBy { get; set; }

    /// <inheritdoc />
    protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
        => builder.AddContent(0, ChildContent(item));

    /// <inheritdoc />
    protected override bool IsSortableByDefault()
        => SortBy is not null;
}
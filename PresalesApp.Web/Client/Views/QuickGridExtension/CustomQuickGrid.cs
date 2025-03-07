using Microsoft.AspNetCore.Components.QuickGrid;

namespace PresalesApp.Web.Client.Views.QuickGridExtension;

public class CustomQuickGrid<TGridItem> : QuickGrid<TGridItem>
{
    public CustomColumnBase<TGridItem> SortByColumn { get; set; }

    public bool SortByAscending { get; set; }

    public new Task SortByColumnAsync(ColumnBase<TGridItem> column, SortDirection direction = SortDirection.Auto)
    {
        SortByAscending = direction switch
        {
            SortDirection.Ascending => true,
            SortDirection.Descending => false,
            SortDirection.Auto => SortByColumn != column || !SortByAscending,
            _ => throw new NotSupportedException($"Unknown sort direction {direction}"),
        };

        SortByColumn = column as CustomColumnBase<TGridItem>;

        return base.SortByColumnAsync(column, direction);
    }
}

using Microsoft.AspNetCore.Components.QuickGrid;

namespace PresalesApp.Web.Client.Views.QuickGridExtension;

public abstract class CustomColumnBase<TGridItem> : ColumnBase<TGridItem>
{
    public new CustomQuickGrid<TGridItem> Grid => base.Grid as CustomQuickGrid<TGridItem>;
}
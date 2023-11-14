using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Views;

public class RadzenDataGridLocalized<TItem> : RadzenDataGrid<TItem>
{
    [Inject]
    protected IStringLocalizer<App> Localization { get; set; }

    protected override void OnInitialized()
    {
        PageSizeText = Localization["DataGridItemsPerPageText"];
        base.OnInitialized();
    }
}

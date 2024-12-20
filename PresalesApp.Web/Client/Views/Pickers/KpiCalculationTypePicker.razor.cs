using Microsoft.AspNetCore.Components;
using KpiCalculation = PresalesApp.Web.Shared.KpiCalculation;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class KpiCalculationTypePicker
{
    [Parameter]
    public EventCallback<KpiCalculation> OnSelectCallback { get; set; }

    [Parameter]
    public KpiCalculation KpiCalculationType { get; set; } = KpiCalculation.Default;

    private void _OnPositionChanged(ChangeEventArgs e)
    {
        if(Enum.TryParse<KpiCalculation>(e?.Value?.ToString(), out var _p))
        {
            KpiCalculationType = _p;
            OnSelectCallback.InvokeAsync(KpiCalculationType);
        }
    }
}

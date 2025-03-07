using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Views.Pickers;

partial class KpiCalculationTypePicker
{
    [Parameter]
    public EventCallback<KpiCalculation> OnSelectCallback { get; set; }

    [Parameter]
    public KpiCalculation KpiCalculationType { get; set; } = KpiCalculation.Default;

    private readonly IEnumerable<KpiCalculation> _CalculationTypes = Enum.GetValues<KpiCalculation>();

    private void _OnCalculationTypeChanged(KpiCalculation calculationType)
    {
        KpiCalculationType = calculationType;
        OnSelectCallback.InvokeAsync(KpiCalculationType);
    }
}

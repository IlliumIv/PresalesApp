using Microsoft.AspNetCore.Components;
using PresalesApp.CustomTypes;
using PresalesApp.Web.Client.Extensions;
using PresalesApp.Web.Client.Views.Pickers.DropDown;

namespace PresalesApp.Web.Client.Views.Pickers;

public partial class PeriodPicker
{
    private List<DropDownItem<DateTime>> _ItemsSet = [];
    private CustomRadzenDropDown<DropDownItem<DateTime>?> _Select;
    private DropDownItem<DateTime>? _SelectedItem;

    [Parameter, EditorRequired]
    public Extensions.Period Period { get; set; }

    [Parameter]
    public EventCallback<Extensions.Period> PeriodChanged { get; set; }

    [Parameter]
    public HashSet<PeriodType> ExcludePeriods { get; set; } = [];

    protected override void OnInitialized() => _GenerateItemsSet(Period.Start, false);

    protected async Task SelectedItemChanged(object item)
    {
        if (_ItemsSet.First() == item || _ItemsSet.Last() == item)
            await _Select.OpenPopupAsync();

        _GenerateItemsSet(((DropDownItem<DateTime>)item).Value);
    }

    private void _GenerateItemsSet(DateTime selectedItemDT, bool shoudInvokeCallback = true)
    {
        switch (Period.Type)
        {
            case PeriodType.Day:
                _GenerateDaysSet(selectedItemDT, shoudInvokeCallback); break;
            case PeriodType.Month:
                _GenerateMonthsSet(selectedItemDT, shoudInvokeCallback); break;
            case PeriodType.Quarter:
                _GenerateQuartersSet(selectedItemDT, shoudInvokeCallback); break;
            case PeriodType.Year:
                _GenerateYearsSet(selectedItemDT, shoudInvokeCallback); break;
            case PeriodType.Arbitrary:
                if (shoudInvokeCallback) PeriodChanged.InvokeAsync(Period); break;
            default: break;
        }
    }

    // private void _ClearSet() => _itemsSet.Clear();

    private void _GenerateDaysSet(DateTime dt, bool shoudInvokeCallback = true)
    {
        List<DropDownItem<DateTime>> result = [];

        // https://stackoverflow.com/a/33526731
        var sunday = dt.AddDays(-(int)dt.DayOfWeek);
        var monday = sunday.AddDays(1);
        // If we started on Sunday, we should actually have gone *back*
        // 6 days instead of forward 1...
        if (dt.DayOfWeek == DayOfWeek.Sunday)
        {
            monday = monday.AddDays(-7);
        }

        for (var i = 0; i <= 7; i++)
        {
            var date = monday.AddDays(i);
            result.Add(new(date,
                date.GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization)));

        }

        result.Add(new(dt.AddDays(-7), Localization["PreviousWeekText"]));
        result.Add(new(dt.AddDays(7), Localization["NextWeekText"]));

        _ItemsSet = [.. result.OrderBy(x => x.Value)];
        _SelectedItem = new(dt, dt.ToString(Helpers.DayFormat).ToUpperFirstLetterString());

        Period = new(dt, PeriodType.Day);
        if (shoudInvokeCallback) PeriodChanged.InvokeAsync(Period);
    }

    private void _GenerateMonthsSet(DateTime dt, bool shoudInvokeCallback = true)
    {
        List<DropDownItem<DateTime>> result = [];

        for (var i = 1; i <= 12; i++)
        {
            var date = new DateTime(dt.Year, i, 1);
            result.Add(new(date,
                date.GetLocalizedDateNameByPeriodType(PeriodType.Month,Localization)));
        }

        result.Add(new(dt.AddMonths(-12),
            $"{dt.AddMonths(-12):yyyy}...".ToUpperFirstLetterString()));
        result.Add(new(dt.AddMonths(12),
            $"{dt.AddMonths(12):yyyy}...".ToUpperFirstLetterString()));

        _ItemsSet = [.. result.OrderBy(x => x.Value)];
        _SelectedItem = new(dt, dt.ToString(Helpers.MonthFormat).ToUpperFirstLetterString());

        Period = new(dt, PeriodType.Month);
        if (shoudInvokeCallback) PeriodChanged.InvokeAsync(Period);
    }

    private void _GenerateQuartersSet(DateTime dt, bool shoudInvokeCallback = true)
    {
        List<DropDownItem<DateTime>> result = [];

        for (var i = 1; i <= 4; i++)
        {
            var date = new DateTime(dt.Year, ((i - 1) * 3) + 1, 1);
            result.Add(new(date,
                date.GetLocalizedDateNameByPeriodType(PeriodType.Quarter, Localization)));
        }

        result.Add(new(dt.AddMonths(-12),
            $"{dt.AddMonths(-12):yyyy}...".ToUpperFirstLetterString()));
        result.Add(new(dt.AddMonths(12),
            $"{dt.AddMonths(12):yyyy}...".ToUpperFirstLetterString()));

        _ItemsSet = [.. result.OrderBy(x => x.Value)];
        _SelectedItem = new(dt,
            $"{Localization["QuarterText", (dt.Month + 2) / 3, dt.Year].Value}"
            .ToUpperFirstLetterString());

        Period = new(dt, PeriodType.Quarter);
        if (shoudInvokeCallback) PeriodChanged.InvokeAsync(Period);
    }

    private void _GenerateYearsSet(DateTime dt, bool shoudInvokeCallback = true)
    {
        List<DropDownItem<DateTime>> result = [];

        for (var i = dt.Year + 3; i >= dt.Year - 3; i--)
        {
            var date = new DateTime(i, dt.Month, 1);
            result.Add(new(date,
                date.GetLocalizedDateNameByPeriodType(PeriodType.Year, Localization)));
        }

        result.Add(new(dt.AddYears(-4), Localization["PreviousYearsText"]));
        result.Add(new(dt.AddYears(4), Localization["NextYearsText"]));

        _ItemsSet = [.. result.OrderBy(x => x.Value)];
        _SelectedItem = new(dt, dt.ToString(Helpers.YearFormat).ToUpperFirstLetterString());
        _Select?.SetSelectedIndex(4);

        Period = new(dt, PeriodType.Year);
        if (shoudInvokeCallback) PeriodChanged.InvokeAsync(Period);
    }

    private void _SelectedPeriodChanged() => _GenerateItemsSet(_SelectedItem?.Value ?? Period.Start);

    private void _OnFromChangedByPicker() => PeriodChanged.InvokeAsync(Period);

    private void _OnToChangedByPicker() => PeriodChanged.InvokeAsync(Period);
}

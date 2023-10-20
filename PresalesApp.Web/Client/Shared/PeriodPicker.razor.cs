using PresalesApp.Web.Client.Shared.DropDown;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using Microsoft.AspNetCore.Components;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Shared
{
    public partial class PeriodPicker
    {
        List<DropDownItem<DateTime>> items_set = [];
        CustomRadzenDropDown<DropDownItem<DateTime>?> select;
        DropDownItem<DateTime>? selected_item;

        [Parameter, EditorRequired]
        public Period Period { get; set; }

        [Parameter]
        public EventCallback<Period> PeriodChanged { get; set; }

        protected override void OnInitialized() => GenerateItemsSet(Period.Start);

        protected async Task SelectedItemChanged(object item)
        {
            if (items_set.First() == item || items_set.Last() == item)
                await select.CustomOpenPopup();

            GenerateItemsSet(((DropDownItem<DateTime>)item).Value);
        }

        void GenerateItemsSet(DateTime selectedItemDT)
        {
            switch (Period.Type)
            {
                case PeriodType.Day: GenerateDaysSet(selectedItemDT); break;
                case PeriodType.Month: GenerateMonthsSet(selectedItemDT); break;
                case PeriodType.Quarter: GenerateQuartersSet(selectedItemDT); break;
                case PeriodType.Year: GenerateYearsSet(selectedItemDT); break;
                // default: ClearSet(); break;
                default: break;
            }
        }

        void ClearSet() => items_set.Clear();

        void GenerateDaysSet(DateTime dt)
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
                result.Add(new(date, date.GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization)));

            }

            result.Add(new(dt.AddDays(-7), Localization["PreviousWeekText"]));
            result.Add(new(dt.AddDays(7), Localization["NextWeekText"]));

            items_set = [.. result.OrderBy(x => x.Value)];
            selected_item = new(dt, dt.ToString(Helpers.Helpers.DayFormat).ToUpperFirstLetterString());

            Period.Start = dt;
            Period.SetPeriodByType(PeriodType.Day);
            PeriodChanged.InvokeAsync(Period);
        }

        void GenerateMonthsSet(DateTime dt)
        {
            List<DropDownItem<DateTime>> result = [];

            for (var i = 1; i <= 12; i++)
            {
                var date = new DateTime(dt.Year, i, 1);
                result.Add(new(date, date.GetLocalizedDateNameByPeriodType(PeriodType.Month, Localization)));
            }
            result.Add(new(dt.AddMonths(-12), $"{dt.AddMonths(-12):yyyy}...".ToUpperFirstLetterString()));
            result.Add(new(dt.AddMonths(12), $"{dt.AddMonths(12):yyyy}...".ToUpperFirstLetterString()));

            items_set = [.. result.OrderBy(x => x.Value)];
            selected_item = new(dt, dt.ToString(Helpers.Helpers.MonthFormat).ToUpperFirstLetterString());

            Period.Start = dt;
            Period.SetPeriodByType(PeriodType.Month);
            PeriodChanged.InvokeAsync(Period);
        }

        void GenerateQuartersSet(DateTime dt)
        {
            List<DropDownItem<DateTime>> result = [];

            for (var i = 1; i <= 4; i++)
            {
                var date = new DateTime(dt.Year, (i - 1) * 3 + 1, 1);
                result.Add(new(date, date.GetLocalizedDateNameByPeriodType(PeriodType.Quarter, Localization)));
            }

            result.Add(new(dt.AddMonths(-12), $"{dt.AddMonths(-12):yyyy}...".ToUpperFirstLetterString()));
            result.Add(new(dt.AddMonths(12), $"{dt.AddMonths(12):yyyy}...".ToUpperFirstLetterString()));

            items_set = [.. result.OrderBy(x => x.Value)];
            selected_item = new(dt, $"{Localization["QuarterText", (dt.Month + 2) / 3, dt.Year].Value}".ToUpperFirstLetterString());

            Period.Start = dt;
            Period.SetPeriodByType(PeriodType.Quarter);
            PeriodChanged.InvokeAsync(Period);
        }

        void GenerateYearsSet(DateTime dt)
        {
            List<DropDownItem<DateTime>> result = [];

            for (var i = dt.Year + 3; i >= dt.Year - 3; i--)
            {
                var date = new DateTime(i, dt.Month, 1);
                result.Add(new(date, date.GetLocalizedDateNameByPeriodType(PeriodType.Year, Localization)));
            }
            result.Add(new(dt.AddYears(-4), Localization["PreviousYearsText"]));
            result.Add(new(dt.AddYears(4), Localization["NextYearsText"]));

            items_set = [.. result.OrderBy(x => x.Value)];
            selected_item = new(dt, dt.ToString(Helpers.Helpers.YearFormat).ToUpperFirstLetterString());
            select.SetSelectedIndex(4);

            Period.Start = dt;
            Period.SetPeriodByType(PeriodType.Year);
            PeriodChanged.InvokeAsync(Period);
        }

        void SelectedPeriodChanged() => GenerateItemsSet(selected_item?.Value ?? Period.Start);

        void OnFromChangedByPicker() => PeriodChanged.InvokeAsync(Period);

        void OnToChangedByPicker() => PeriodChanged.InvokeAsync(Period);
    }
}

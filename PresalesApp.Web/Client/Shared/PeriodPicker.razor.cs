using PresalesApp.Web.Client.Shared.DropDown;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;

namespace PresalesApp.Web.Client.Shared
{
    public partial class PeriodPicker
    {
        static readonly string dayFormat = "ddd, dd MMMM yyyy";
        static readonly string monthFormat = "MMMM yyyy";
        static readonly string yearFormat = "yyyy";
        static readonly IEnumerable<PeriodType> periodTypes = Enum.GetValues(typeof(PeriodType)).Cast<PeriodType>();

        static PeriodType selectedPeriod = PeriodType.Arbitrary;

        DateTime begin = DateTime.Now;

        DateTime end = DateTime.Now;

        List<DropDownItem<DateTime>> items = new();

        CustomRadzenDropDown<DropDownItem<DateTime>?> itemsDropDown;

        DropDownItem<DateTime>? selectedItem;

        protected override void OnInitialized()
        {
            selectedItem = new(DateTime.Now, DateTime.Now.ToString(dayFormat).ToUpperFirstLetterString());
            items = GenerateItems(DateTime.Now, selectedPeriod);
        }

        protected async Task DropDown1Change(object item)
        {
            if (items.First() == item || items.Last() == item)
            {
                items = GenerateItems(((DropDownItem<DateTime>)item).Value, selectedPeriod);
                selectedItem = null;
                await itemsDropDown.CustomOpenPopup();
            }
        }

        List<DropDownItem<DateTime>> GenerateItems(DateTime start, PeriodType selectedPeriod)
        {
            Console.WriteLine("GenerateItems");

            List<DropDownItem<DateTime>> result = new();
            begin = start;

            switch (selectedPeriod)
            {
                case PeriodType.Day:
                    // https://stackoverflow.com/a/33526731
                    var sunday = start.AddDays(-(int)start.DayOfWeek);
                    var monday = sunday.AddDays(1);
                    // If we started on Sunday, we should actually have gone *back*
                    // 6 days instead of forward 1...
                    if (start.DayOfWeek == DayOfWeek.Sunday)
                    {
                        monday = monday.AddDays(-7);
                    }

                    for (var i = 0; i <= 7; i++)
                    {
                        var date = monday.AddDays(i);
                        result.Add(new(date, date.ToString(dayFormat).ToUpperFirstLetterString()));

                    }

                    result.Add(new(start.AddDays(-7), Localization["PreviousWeekText"]));
                    result.Add(new(start.AddDays(7), Localization["NextWeekText"]));
                    selectedItem = new(start, start.ToString(dayFormat).ToUpperFirstLetterString());
                    end = start.AddDays(1).AddSeconds(-1);
                    break;
                case PeriodType.Month:
                    for (var i = 1; i <= 12; i++)
                    {
                        var date = new DateTime(start.Year, i, 1);
                        result.Add(new(date, date.ToString(monthFormat).ToUpperFirstLetterString()));
                    }
                    result.Add(new(start.AddMonths(-12), $"{start.AddMonths(-12):yyyy}...".ToUpperFirstLetterString()));
                    result.Add(new(start.AddMonths(12), $"{start.AddMonths(12):yyyy}...".ToUpperFirstLetterString()));
                    selectedItem = new(start, start.ToString(monthFormat).ToUpperFirstLetterString());
                    end = start.AddMonths(1).AddSeconds(-1);
                    break;
                case PeriodType.Quarter:
                    for (var i = 1; i <= 4; i++)
                    {
                        var date = new DateTime(start.Year, (i - 1) * 3 + 1, 1);
                        result.Add(new(date, $"{Localization["QuarterText", i, date.Year].Value}".ToUpperFirstLetterString()));
                    }
                    result.Add(new(start.AddMonths(-12), $"{start.AddMonths(-12):yyyy}...".ToUpperFirstLetterString()));
                    result.Add(new(start.AddMonths(12), $"{start.AddMonths(12):yyyy}...".ToUpperFirstLetterString()));
                    selectedItem = new(start, $"{Localization["QuarterText", (start.Month + 2) / 3, start.Year].Value}".ToUpperFirstLetterString());
                    end = start.AddMonths(3).AddSeconds(-1);
                    break;
                case PeriodType.Year:
                    for (var i = start.Year + 3; i >= start.Year - 3; i--)
                    {
                        var date = new DateTime(i, start.Month, 1);
                        result.Add(new(date, date.ToString(yearFormat).ToUpperFirstLetterString()));
                    }
                    result.Add(new(start.AddYears(-4), Localization["PreviousYearsText"]));
                    result.Add(new(start.AddYears(4), Localization["NextYearsText"]));
                    selectedItem = new(start, start.ToString(yearFormat).ToUpperFirstLetterString());
                    end = start.AddYears(1).AddSeconds(-1);
                    break;
                default: break;
            }

            return result.OrderBy(x => x.Value).ToList();
        }

        protected void DropDown0Change(object args) =>
            items = GenerateItems(selectedItem?.Value ?? DateTime.Now, selectedPeriod);
    }
}

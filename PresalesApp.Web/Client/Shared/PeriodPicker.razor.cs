using PresalesApp.Web.Client.Shared.DropDown;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace PresalesApp.Web.Client.Shared
{
    public partial class PeriodPicker
    {
        static readonly string dayFormat = "ddd, dd MMMM yyyy";
        static readonly string monthFormat = "MMMM yyyy";
        static readonly string yearFormat = "yyyy";
        static readonly IEnumerable<PeriodType> periodTypes = Enum.GetValues(typeof(PeriodType)).Cast<PeriodType>();

        [Parameter, SupplyParameterFromQuery]
        public string? period { get; set; }
        PeriodType SelectedPeriod { get; set; }

        [Parameter, SupplyParameterFromQuery]
        public string? from { get; set; }

        [Parameter]
        public EventCallback<DateTime> FromChangedCallback { get; set; }

        public DateTime From
        {
            get
            {
                return DateTime.TryParse(from, CultureInfo.InvariantCulture, out var result) ? result : DateTime.Now;
            }
            set
            {
                from = value.ToString(CultureInfo.InvariantCulture);
                Navigation.NavigateTo(
                    Navigation.GetUriWithQueryParameters(
                        new Dictionary<string, object?>
                        {
                            ["from"] = From.ToString(CultureInfo.InvariantCulture),
                            ["to"] = To.ToString(CultureInfo.InvariantCulture),
                            ["period"] = SelectedPeriod.ToString(),
                        }));
                FromChangedCallback.InvokeAsync(value);
            }
        }

        [Parameter, SupplyParameterFromQuery]
        public string? to { get; set; }

        [Parameter]
        public EventCallback<DateTime> ToChangedCallback { get; set; }

        public DateTime To
        {
            get
            {
                return DateTime.TryParse(to, CultureInfo.InvariantCulture, out var result) ? result : DateTime.Now;
            }
            set
            {
                to = value.ToString(CultureInfo.InvariantCulture);
                Navigation.NavigateTo(
                    Navigation.GetUriWithQueryParameters(
                        new Dictionary<string, object?>
                        {
                            ["from"] = From.ToString(CultureInfo.InvariantCulture),
                            ["to"] = To.ToString(CultureInfo.InvariantCulture),
                            ["period"] = SelectedPeriod.ToString(),
                        }));
                ToChangedCallback.InvokeAsync(value);
            }
        }

        List<DropDownItem<DateTime>> items = [];
        CustomRadzenDropDown<DropDownItem<DateTime>?> itemsDropDown;
        DropDownItem<DateTime>? selectedItem;

        protected override void OnInitialized()
        {
            SelectedPeriod = Enum.TryParse(period, out PeriodType type) ? type : PeriodType.Arbitrary;
            selectedItem = new(DateTime.Now, DateTime.Now.ToString(dayFormat).ToUpperFirstLetterString());
            items = GenerateItems(From, SelectedPeriod);
        }

        protected async Task DropDown1Change(object item)
        {
            if (items.First() == item || items.Last() == item)
                await itemsDropDown.CustomOpenPopup();

            items = GenerateItems(((DropDownItem<DateTime>)item).Value, SelectedPeriod);
        }

        List<DropDownItem<DateTime>> GenerateItems(DateTime start, PeriodType selectedPeriod)
        {
            List<DropDownItem<DateTime>> result = [];

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
                    From = start.Date;
                    To = From.AddDays(1).AddSeconds(-1);
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
                    From = new DateTime(start.Year, start.Month, 1);
                    To = From.AddMonths(1).AddSeconds(-1);
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
                    int quarterNumber = (start.Month - 1) / 3 + 1;
                    From = new DateTime(start.Year, (quarterNumber - 1) * 3 + 1, 1);
                    To = From.AddMonths(3).AddSeconds(-1);
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
                    From = new DateTime(start.Year, 1, 1);
                    To = From.AddYears(1).AddSeconds(-1);
                    break;
                default:
                    break;
            }

            // PeriodChanged();

            return [.. result.OrderBy(x => x.Value)];
        }

        protected void SelectedPeriodChanged()
        {
            items = GenerateItems(selectedItem?.Value ?? From, SelectedPeriod);
        }
    }
}

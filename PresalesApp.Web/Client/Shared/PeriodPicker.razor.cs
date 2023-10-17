using PresalesApp.Web.Client.Shared.DropDown;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace PresalesApp.Web.Client.Shared
{
    public partial class PeriodPicker
    {
        const string dayFormat = "ddd, dd MMMM yyyy";
        const string monthFormat = "MMMM yyyy";
        const string yearFormat = "yyyy";
        const string queryDateTimeFormat = "yyyy-MM-ddTHH-mm-ss";
        readonly IEnumerable<PeriodType> periodTypes = Enum.GetValues(typeof(PeriodType)).Cast<PeriodType>();

        [Parameter, EditorRequired]
        public Dictionary<string, object?> Params { get; set; }

        #region From
        string? from { get; set; }

        [Parameter, EditorRequired]
        public string FromQueryName { get; set; }

        [Parameter]
        public EventCallback<DateTime> FromChanged { get; set; }

        public DateTime From
        {
            get
            {
                if (DateTime.TryParseExact(from, queryDateTimeFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) return result;
                From = DateTime.Now;
                return From;
            }
            set
            {
                from = value.ToString(queryDateTimeFormat);
                Params[FromQueryName] = from;
                Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(Params));
                FromChanged.InvokeAsync(value);
            }
        }
        #endregion

        #region To
        string? to { get; set; }

        [Parameter, EditorRequired]
        public string ToQueryName { get; set; }

        [Parameter]
        public EventCallback<DateTime> ToChanged { get; set; }

        public DateTime To
        {
            get
            {
                if (DateTime.TryParseExact(to, queryDateTimeFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) return result;
                To = DateTime.Now;
                return To;
            }
            set
            {
                to = value.ToString(queryDateTimeFormat);
                Params[ToQueryName] = to;
                Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(Params));
                ToChanged.InvokeAsync(value);
            }
        }
        #endregion

        #region Period
        string? period { get; set; }

        [Parameter, EditorRequired]
        public string PeriodQueryName { get; set; }

        [Parameter]
        public EventCallback<PeriodType> PeriodChanged { get; set; }

        public PeriodType Period
        {
            get
            {
                if (Enum.TryParse(period, out PeriodType type)) return type;
                Period = PeriodType.Arbitrary;
                return Period;
            }
            set
            {
                period = value.ToString();
                Params[PeriodQueryName] = period;
                Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(Params));
                PeriodChanged.InvokeAsync(value);
            }
        }
        #endregion

        List<DropDownItem<DateTime>> items = [];
        CustomRadzenDropDown<DropDownItem<DateTime>?> itemsDropDown;
        DropDownItem<DateTime>? selectedItem;

        protected override void OnInitialized()
        {
            selectedItem = new(DateTime.Now, DateTime.Now.ToString(dayFormat).ToUpperFirstLetterString());
            items = GenerateItems(From, Period);
        }

        protected async Task DropDown1Change(object item)
        {
            if (items.First() == item || items.Last() == item)
                await itemsDropDown.CustomOpenPopup();

            items = GenerateItems(((DropDownItem<DateTime>)item).Value, Period);
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
            items = GenerateItems(selectedItem?.Value ?? From, Period);
        }
    }
}

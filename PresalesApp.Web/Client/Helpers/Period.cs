using Google.Protobuf.WellKnownTypes;
using PresalesApp.Web.Client.Enums;

namespace PresalesApp.Web.Client.Helpers
{
    public class Period(DateTime start, DateTime end, PeriodType periodType = PeriodType.Arbitrary)
    {
        public DateTime Start = start;
        public DateTime End = end;
        public PeriodType Type = periodType;

        public Period(DateTime start, PeriodType periodType)
            : this(start, periodType switch
            {
                PeriodType.Day => AddDay(start),
                PeriodType.Month => AddMonth(start),
                PeriodType.Quarter => AddQuarter(start),
                PeriodType.Year => AddYear(start),
                PeriodType.Arbitrary => start,
                _ => throw new NotImplementedException()
            }, periodType) { }

        public Web.Shared.Period Translate() => new()
        {
            From = Timestamp.FromDateTime(Start.ToUniversalTime()),
            To = Timestamp.FromDateTime(End.ToUniversalTime())
        };

        private static DateTime AddDay(DateTime dt) => dt.AddDays(1).AddSeconds(-1);
        private static DateTime AddMonth(DateTime dt) => dt.AddMonths(1).AddSeconds(-1);
        private static DateTime AddQuarter(DateTime dt) => dt.AddMonths(3).AddSeconds(-1);
        private static DateTime AddYear(DateTime dt) => dt.AddYears(1).AddSeconds(-1);

        public void SetPeriodByType(PeriodType type)
        {
            switch (type)
            {
                case PeriodType.Day:
                    Start = Start.Date;
                    End = Start.AddDays(1).AddSeconds(-1);
                    break;
                case PeriodType.Month:
                    Start = new DateTime(Start.Year, Start.Month, 1);
                    End = Start.AddMonths(1).AddSeconds(-1);
                    break;
                case PeriodType.Quarter:
                    Start = new DateTime(Start.Year, (Start.Month - 1) / 3 * 3 + 1, 1);
                    End = Start.AddMonths(3).AddSeconds(-1);
                    break;
                case PeriodType.Year:
                    Start = new DateTime(Start.Year, 1, 1);
                    End = Start.AddYears(1).AddSeconds(-1);
                    break;
                case PeriodType.Arbitrary: break;
                default: throw new NotImplementedException();
            }
        }
    }
}

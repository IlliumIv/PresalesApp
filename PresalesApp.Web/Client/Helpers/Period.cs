using Google.Protobuf.WellKnownTypes;
using PresalesApp.CustomTypes;

namespace PresalesApp.Web.Client.Helpers;

public class Period(DateTime start, DateTime end, PeriodType periodType = PeriodType.Arbitrary)
{
    public DateTime Start = start;

    public DateTime End = end;

    public PeriodType Type = periodType;

    public Period(DateTime start, PeriodType periodType)
        : this(start, periodType switch
        {
            PeriodType.Day => _AddDay(start),
            PeriodType.Month => _AddMonth(start),
            PeriodType.Quarter => _AddQuarter(start),
            PeriodType.Year => _AddYear(start),
            PeriodType.Arbitrary => start,
            _ => throw new NotImplementedException()
        }, periodType)
    { }

    public CustomTypes.Period Translate() => new()
    {
        From = Timestamp.FromDateTime(Start.ToUniversalTime()),
        To = Timestamp.FromDateTime(End.ToUniversalTime())
    };

    private static DateTime _AddDay(DateTime dt) => dt.AddDays(1).AddSeconds(-1);

    private static DateTime _AddMonth(DateTime dt) => dt.AddMonths(1).AddSeconds(-1);

    private static DateTime _AddQuarter(DateTime dt) => dt.AddMonths(3).AddSeconds(-1);

    private static DateTime _AddYear(DateTime dt) => dt.AddYears(1).AddSeconds(-1);

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
                Start = new DateTime(Start.Year, ((Start.Month - 1) / 3 * 3) + 1, 1);
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

using Blazored.LocalStorage;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using PresalesApp.Shared;
using PresalesApp.ImageProvider;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using PresalesApp.CustomTypes;
using System.Reflection;

namespace PresalesApp.Web.Client.Extensions;

public static partial class Helpers
{
    public const string DayFormat = "ddd, dd MMMM yyyy";

    public const string MonthFormat = "MMMM yyyy";

    public const string YearFormat = "yyyy";

    public const string UriDateTimeFormat = "yyyyMMddTHHmmss"; // ISO_8601

    public const string DateTimeFormat = "dd.MM.yyyy HH:mm:ss";

    public static string GetLocalizedCurrentMonthName() => $"{DateTime.Now:MMMM}";

    [GeneratedRegex("\\s+")]
    private static partial Regex _DeleteMultipleSpaces();

    public static string GetFirstAndLastName(this string name)
        => string.Join(" ", _DeleteMultipleSpaces().Replace(name, " ").Split().Take(2));

    public static string ToMinMaxFormatString(DateTime? value) => $"{value:yyyy-MM-dd}";

    public static string ToCurrencyString(this DecimalValue? value, bool allowNegatives = false,
        bool shortFormat = false, CultureInfo? cultureInfo = null, string? groupSeparator = null)
        => ToCurrencyString((decimal)value, allowNegatives, shortFormat, cultureInfo, groupSeparator);

    public static string ToCurrencyString(this decimal value, bool allowNegatives = false,
        bool shortFormat = false, CultureInfo? cultureInfo = null, string? groupSeparator = null)
    {
        cultureInfo ??= new CultureInfo("ru-RU");
        var numberFormat = cultureInfo.NumberFormat;
        numberFormat.NumberGroupSeparator = groupSeparator ?? numberFormat.NumberGroupSeparator;
        numberFormat.CurrencyGroupSeparator = groupSeparator ?? numberFormat.CurrencyGroupSeparator;
        numberFormat.PercentGroupSeparator = groupSeparator ?? numberFormat.PercentGroupSeparator;

        cultureInfo.NumberFormat = numberFormat;

        var result = shortFormat switch
        {
            false => value.ToString("N2", cultureInfo),
            true => value.ToString(value._GetShortener(), cultureInfo)
        };

        return string.Format(cultureInfo, value._GetPattern(cultureInfo, allowNegatives),
                             numberFormat.CurrencySymbol, result);
    }

    private static string _GetShortener(this decimal value) => Math.Abs(value) switch
    {
        > 1_000_000_000 => "0,,,.###B",
        > 1_000_000 => "0,,.##M",
        > 1_000 => "0,.#K",
        _ => "0.#"
    };

    private static string _GetPattern(this decimal value,
        CultureInfo cultureInfo, bool allowNegatives) => (value >= decimal.Zero) switch
    {
        true => cultureInfo.NumberFormat.CurrencyPositivePattern switch
        {
            0 => "{0}{1}",
            1 => "{1}{0}",
            2 => "{0} {1}",
            3 => "{1} {0}",
            _ => throw new NotImplementedException()
        },
        false => allowNegatives switch
        {
            true => cultureInfo.NumberFormat.CurrencyNegativePattern switch
            {
                0 => "({0}{1})",
                1 => "{0}{1}",
                2 => "{0}{1}",
                3 => "{0}{1}",
                4 => "({1}{0})",
                5 => "{1}{0}",
                6 => "{1}{0}",
                7 => "{1}{0}",
                8 => "{1} {0}",
                9 => "{0} {1}",
                10 => "{1} {0}",
                11 => "{0} {1}",
                12 => "{0} {1}",
                13 => "{1} {0}",
                14 => "({0} {1})",
                15 => "({1} {0})",
                _ => throw new NotImplementedException(),
            },
            false => ""
        }
    };

    public static string ToPercentString(this double value, int digits = 0)
        => value == 0 ? "" : string.Format($"{{0:P{digits}}}", value);

    public static string ToPercentString(this decimal value, int digits = 0)
        => value == 0 ? "" : string.Format($"{{0:P{digits}}}", value);

    public static string ToDaysString(TimeSpan avgTTW)
        => avgTTW == TimeSpan.Zero ? "" : $"{avgTTW.TotalDays:f0}";

    public static string ToMinutesString(TimeSpan avgTTR)
        => avgTTR == TimeSpan.Zero ? "" : $"{avgTTR.TotalMinutes:f0}";

    public static string ToHoursString(TimeSpan timeSpend)
        => timeSpend == TimeSpan.Zero ? "" : $"{timeSpend.TotalHours:f1}";

    public static string ToDateString(Timestamp timestamp, string separator)
        => timestamp.ToDateTime() == DateTime.MinValue ? "" : $"{separator}{timestamp.ToDateTime().ToPresaleTime()}";

    public static string ToOneDateString(Timestamp a, Timestamp b)
        => $"{(a.ToDateTime() == DateTime.MinValue
            ? b.ToDateTime().ToPresaleTime()
            : a.ToDateTime().ToPresaleTime())}";

    public static string ToUpperFirstLetterString(this string value)
        => value.Length switch
        {
            0 => value,
            1 => char.ToUpper(value[0]).ToString(),
            _ => char.ToUpper(value[0]) + value[1..]
        };

    public static string ToEmptyIfZeroString(double? value)
        => value switch
        {
            0 => "",
            _ => $"{value}"
        };

    public static string ToEmptyIfZeroString(int? value)
        => value switch
        {
            0 => "",
            _ => $"{value}"
        };

    private async static Task _SaveAs(IJSRuntime js, string filename, byte[] data)
        => await js.InvokeAsync<object>("SaveAsFile", filename, Convert.ToBase64String(data));

    public async static Task SetElementMinWidthById (IJSRuntime js, string id, string width)
        => await js.InvokeAsync<object>("SetMinWidth", id, width);

    public async static Task Download(this Kpi? kpi, IJSRuntime js,
        string presaleName, Period period, IStringLocalizer<App> localization)
    {
        if (kpi == null) return;

        var text = $"{localization["OrderNumberText"]};" +
            $"{localization["CounterpartText"]};" +
            $"{localization["InvoiceNumberText"]};" +
            $"{localization["InvoiceDateText"]};" +
            $"{localization["InvoiceAmountText"]};" +
            $"{localization["InvoiceCostPriceText"]};" +
            $"{localization["InvoiceProfitText"]};" +
            $"{localization["PresalePercentText"]};" +
            $"{localization["PresaleProfitText"]};" +
            $"{localization["ProjectsText"]}\n";

        var i = 0;

        foreach (var invoice in kpi.Invoices)
        {
            i++;
            text += $"{i};{invoice.Counterpart};{invoice.Number};" +
                $"{invoice.Date.ToDateTime().ToPresaleTime()};{(decimal)invoice.Amount};" +
                $"{(decimal)invoice.Cost};{(decimal)invoice.SalesAmount};" +
                $"{(invoice.Percent > 0 ? invoice.Percent * 100 : "")};" +
                $"{((decimal)invoice.Profit > 0 ? (decimal)invoice.Profit : "")};";

            foreach (var project in invoice.ProjectsFound)
            {
                text += $"{project.Number}, ";
            }

            if (invoice.ProjectsFound.Count != 0) text = text[..^2];
            text += "\n";
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        byte[] result = [.. Encoding.UTF8.GetPreamble(), .. bytes];
        UTF8Encoding encoder = new(true);
        text = encoder.GetString(result);

        await _SaveAs(js, $"{localization["KpiReportFileName",
            period.GetLocalizedPeriodName(localization, false),
            presaleName.GetFirstAndLastName()]
            .Value}.csv", Encoding.UTF8.GetBytes(text));
    }

    public static PropertyInfo[] GetProperties<T>()
        => typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

    public async static Task DownloadReport(string text, IJSRuntime js, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        byte[] result = [.. Encoding.UTF8.GetPreamble(), .. bytes];
        UTF8Encoding encoder = new(true);
        text = encoder.GetString(result);
        await _SaveAs(js, fileName, Encoding.UTF8.GetBytes(text));
    }

    public static string GetLocalizedPeriodName(this Period period,
        IStringLocalizer<App> localization, bool showTime = true)
            => period.Type switch
        {
            PeriodType.Day => GetLocalizedDateNameByPeriodType
                (period.Start, period.Type, localization, showTime),
            PeriodType.Month => GetLocalizedDateNameByPeriodType
                (period.Start, period.Type, localization, showTime),
            PeriodType.Quarter => GetLocalizedDateNameByPeriodType
                (period.Start, period.Type, localization, showTime),
            PeriodType.Year => GetLocalizedDateNameByPeriodType
                (period.Start, period.Type, localization, showTime),

            _ => $"{(showTime ? $"{period.Start}" : $"{period.Start:d}")} - " +
                 $"{(showTime ? $"{period.End}" : $"{period.End:d}")}"
                .ToUpperFirstLetterString()
        };

    public static string GetLocalizedDateNameByPeriodType(this DateTime dt,
        PeriodType periodType, IStringLocalizer<App> localization, bool showTime = true)
        => periodType switch
        {
            PeriodType.Day => dt.ToString(DayFormat).ToUpperFirstLetterString(),
            PeriodType.Month => dt.ToString(MonthFormat).ToUpperFirstLetterString(),
            PeriodType.Quarter => $"{localization["QuarterText",
                ((dt.Month - 1) / 3) + 1, dt.Year].Value}".ToUpperFirstLetterString(),
            PeriodType.Year => dt.ToString(YearFormat).ToUpperFirstLetterString(),
            _ => $"{(showTime ? $"{dt}" : $"{dt:d}")}"
        };

    public static string GetLocalizedName(this Department department,
        IStringLocalizer<App> localization) => department switch
    {
        Department.None => localization["DepartmentNoneText"],
        Department.Russian => localization["DepartmentRussianText"],
        Department.International => localization["DepartmentInternationalText"],
        Department.Any => localization["DepartmentAnyText"],
        _ => throw new NotImplementedException()
    };

    public static string GetLocalizedName(this FunnelStage stage,
        IStringLocalizer<App> localization) => stage switch
    {
        FunnelStage.None => localization["FunnelStageNone"],
        FunnelStage.First => localization["FunnelStageFirst"],
        FunnelStage.Second => localization["FunnelStageSecond"],
        FunnelStage.Third => localization["FunnelStageThird"],
        FunnelStage.Fourth => localization["FunnelStageFourth"],
        FunnelStage.Fifth => localization["FunnelStageFifth"],
        FunnelStage.Sixth => localization["FunnelStageSixth"],
        FunnelStage.Seventh => localization["FunnelStageSeventh"],
        FunnelStage.Eigth => localization["FunnelStageEigth"],
        FunnelStage.Refused => localization["FunnelStageRefused"],
        FunnelStage.Any => localization["FunnelStageAny"],
        _ => throw new NotImplementedException()
    };

    public static string GetLocalizedName(this Position position,
        IStringLocalizer<App> localization) => position switch
    {
        Position.None => localization["PositionNoneText"],
        Position.Account => localization["PositionAccountText"],
        Position.Engineer => localization["PositionEngineerText"],
        Position.Director => localization["PositionDirectorText"],
        Position.Any => localization["PositionAnyText"],
        _ => throw new NotImplementedException()
    };

    public static string GetLocalizedName(this ProjectStatus status,
        IStringLocalizer<App> localization) => status switch
    {
        ProjectStatus.None => localization["ProjectStatusNoneText"],
        ProjectStatus.WorkInProgress => localization["ProjectStatusWorkInProgressText"],
        ProjectStatus.Won => localization["ProjectStatusWonText"],
        ProjectStatus.Loss => localization["ProjectStatusLossText"],
        _ => throw new NotImplementedException()
    };

    public static string GetLocalizedName(this PeriodType periodType,
        IStringLocalizer<App> localization) => periodType switch
    {
        PeriodType.Day => localization["PeriodTypeDayText"],
        PeriodType.Month => localization["PeriodTypeMonthText"],
        PeriodType.Quarter => localization["PeriodTypeQuarterText"],
        PeriodType.Year => localization["PeriodTypeYearText"],
        PeriodType.Arbitrary => localization["PeriodTypeArbitraryText"],
        _ => throw new NotImplementedException()
    };

    public static string GetLocalizedName(this KpiCalculation kpiCalculationType,
        IStringLocalizer<App> localization) => kpiCalculationType switch
    {
        KpiCalculation.Default => localization["KpiCalculationTypeDefaultText"],
        KpiCalculation.PreOrder26 => localization["KpiCalculationTypePreOrder26Text"],
        KpiCalculation.PostOrder26 => localization["KpiCalculationTypePostOrder26Text"],
        _ => throw new NotImplementedException()
    };

    // TODO: Реализовать поддержку рабочего времени пресейла.
    public static DateTime ToPresaleTime(this DateTime dateTime)
        => dateTime == DateTime.MinValue ? DateTime.MinValue : dateTime.ToLocalTime();

    public static DateTime ToPresaleTime(this Timestamp timestamp)
        => ToPresaleTime(timestamp.ToDateTime());

    public static string Format(this PresalesApp.Shared.Action action,
        IStringLocalizer<App> localization)
        => $"{action.ProjectNumber} [{action.Type}" +
            $"{ToDateString(action.Date, ": ")}" +
            $" ({action.Timespend.ToTimeSpan().TotalMinutes})], \"{action.Description}\"" +
            (action.SalesFunnel ? $" ({localization["ActionSalesFunnelMarkText"]})." : ".");

    public static string Format(this Project project, IStringLocalizer<App> localization)
        => $"{project.Number} [{project.Status.GetLocalizedName(localization)}" +
            $"{ToDateString(project.ClosedAt, ": ")}" +
            $"], \"{project.Name}\"";

    public static string SetColor(Invoice invoice)
        => invoice.ProjectsIgnored.Count != 0
        || invoice.ActionsIgnored.Count != 0
        || (decimal)invoice.Profit == 0 ? "red" : "inherit";

    public static void SetFromQueryOrStorage(string? value, string query,
        string uri, ISyncLocalStorageService storage, ref DateTime param)
    {
        if (DateTime.TryParseExact(value, UriDateTimeFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var p))
        {
            param = p;
            storage.SetItem($"{new Uri(uri).LocalPath}.{query}", param);
        }
        else if (storage.ContainKey($"{new Uri(uri).LocalPath}.{query}"))
        {
            param = storage.GetItem<DateTime>($"{new Uri(uri).LocalPath}.{query}");
        }
    }

    public static void SetFromQueryOrStorage<TEnum>(string? value, string query,
        string uri, ISyncLocalStorageService storage, ref TEnum param)
    where TEnum : struct
    {
        if (System.Enum.TryParse(value, out TEnum p))
        {
            param = p;
            storage.SetItem($"{new Uri(uri).LocalPath}.{query}", param);
        }
        else if (storage.ContainKey($"{new Uri(uri).LocalPath}.{query}"))
        {
            param = storage.GetItem<TEnum>($"{new Uri(uri).LocalPath}.{query}");
        }
    }

    public static void SetFromQueryOrStorage(string? value, string query,
        string uri, ISyncLocalStorageService storage, ref bool param)
    {
        if (bool.TryParse(value, out var p))
        {
            param = p;
            storage.SetItem($"{new Uri(uri).LocalPath}.{query}", param);
        }
        else if (storage.ContainKey($"{new Uri(uri).LocalPath}.{query}"))
        {
            param = storage.GetItem<bool>($"{new Uri(uri).LocalPath}.{query}");
        }
    }

    public static void SetFromQueryOrStorage(string? value, string query,
        string uri, ISyncLocalStorageService storage, ref string param)
    {
        if (value is not null && !string.IsNullOrEmpty(value))
        {
            param = value;
            storage.SetItemAsString($"{new Uri(uri).LocalPath}.{query}", param);
        }
        else if (storage.ContainKey($"{new Uri(uri).LocalPath}.{query}"))
        {
            param = storage.GetItemAsString($"{new Uri(uri).LocalPath}.{query}") ?? string.Empty;
        }
    }

    public static DateTime StartOfDay(this DateTime date)
        => new(date.Year, date.Month, date.Day, 0, 0, 0, 0);

    public static string GetIcon(KeywordType type)
        => type switch { KeywordType.Collections => "photo_library", _ => "photo" };

    public static decimal ParseOrDefault(this string? s) => decimal.TryParse(s, out var result) ? result : 0;
}
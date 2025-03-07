using Google.Protobuf.WellKnownTypes;
using PresalesApp.CustomTypes;
using PresalesApp.Shared;

namespace PresalesApp.Web.Client.Extensions;

public static class DataGridFilters
{
    public static bool PercentFilter(object doubleValue, object searchValue)
        => !double.TryParse(searchValue?.ToString(), out var filter)
            || (double)doubleValue * 100 >= filter;

    public static bool DecimalValueFilter(object decimalValue, object searchValue)
        => !decimal.TryParse(searchValue?.ToString(), out var filter)
            || (decimal)(DecimalValue)decimalValue >= filter;

    public static bool StatisticByProfitFilter(object metrics, object searchValue)
        => !decimal.TryParse(searchValue?.ToString(), out var filter)
            || (decimal)((Metrics)metrics).Profit >= filter;

    public static bool PresaleFilter(object itemValue, object searchValue)
        => ((Presale)itemValue)?.Name?.Contains(searchValue?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase) ?? true;

    public static bool GetDateTimeFilter(object itemValue, object searchValue)
        => DefaultFilter(((Timestamp)itemValue).ToDateTime().ToPresaleTime(), searchValue);

    public static bool DefaultFilter(object itemValue, object searchValue)
        => searchValue is not string defaultFilter
            || (itemValue.ToString()?.Contains(defaultFilter,
                StringComparison.OrdinalIgnoreCase) ?? true);
}

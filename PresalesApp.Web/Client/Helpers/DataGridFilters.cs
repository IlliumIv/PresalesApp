using Google.Protobuf.WellKnownTypes;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Helpers
{
    public static class DataGridFilters
    {
        public static bool PercentFilter(object doubleValue, object searchValue)
        {
            var @double = (double)doubleValue * 100;
            if (double.TryParse(searchValue?.ToString(), out var filter))
            {
                return @double >= filter;
            }
            return true;
        }
        public static bool DecimalValueFilter(object decimalValue, object searchValue)
        {
            var @decimal = (decimal)(DecimalValue)decimalValue;
            if (decimal.TryParse(searchValue?.ToString(), out var filter))
            {
                return @decimal >= filter;
            }
            return true;
        }

        public static bool PresaleFilter(object itemValue, object searchValue)
        {
            var presale = (Presale)itemValue;
            var search = searchValue?.ToString() ?? string.Empty;
            return presale?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? true;
        }

        public static bool DateTimeFilter(object itemValue, object searchValue) =>
            DefaultFilter(((Timestamp)itemValue).ToDateTime().ToPresaleTime(), searchValue);

        public static bool DefaultFilter(object itemValue, object searchValue)
        {
            if (searchValue is string defaultFilter)
            {
                return itemValue.ToString()?.Contains(defaultFilter, StringComparison.OrdinalIgnoreCase) ?? true;
            }

            return true;
        }
    }
}

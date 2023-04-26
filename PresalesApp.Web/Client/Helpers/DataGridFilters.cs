using Google.Protobuf.WellKnownTypes;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Helpers
{
    public static class DataGridFilters
    {
        public static bool OnPotentialFilter(object itemValue, object searchValue)
        {
            var potential = (decimal)((DecimalValue)itemValue);

            if (decimal.TryParse(searchValue?.ToString(), out var filter))
            {
                return potential > filter;
            }

            return true;
        }

        public static bool OnPresaleFilter(object itemValue, object searchValue)
        {
            var presale = (Presale)itemValue;
            var search = searchValue?.ToString() ?? string.Empty;
            return presale?.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? true;
        }

        public static bool OnDateTimeFilter(object itemValue, object searchValue) =>
            OnDefaultFilter(((Timestamp)itemValue).ToDateTime().ToPresaleTime(), searchValue);

        public static bool OnDefaultFilter(object itemValue, object searchValue)
        {
            if (searchValue is string defaultFilter)
            {
                return itemValue.ToString()?.Contains(defaultFilter, StringComparison.OrdinalIgnoreCase) ?? true;
            }

            return true;
        }
    }
}

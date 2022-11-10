namespace Entities.Helpers
{
    public static class NullableDateTimeExtension
    {
        public static DateTime? ToLocal(this DateTime? dt)
        {
            if (dt == null) return null;
            var ts = (DateTime)dt;
            return ts.TimeOfDay == TimeSpan.Zero ? ts : ts.ToLocalTime();
        }
        public static double? TotalDays(this TimeSpan? ts)
        {
            if (ts == null) return null;
            var nonNullableTS = (TimeSpan)ts;
            return nonNullableTS.TotalDays;
        }
    }
}

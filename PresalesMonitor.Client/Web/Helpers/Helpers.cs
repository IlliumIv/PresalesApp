using System.Globalization;
using System.Runtime.Serialization;

namespace PresalesMonitor.Client.Web.Helpers
{
    public static class Helpers
    {
        public static string ToMinMaxFormatString(DateOnly? value) => $"{value:yyyy-MM-dd}";
        public static string ToCurrencyString(decimal value) => $"{(value > 0 ? value : ""):C}";
        public static string ToPercentString(double value, int digits = 0) => $"{(value == 0 ? "" : value.ToString($"P{digits}"))}";
        public static string ToDaysString(TimeSpan avgTTW) => $"{(avgTTW == TimeSpan.Zero ? "" : avgTTW.TotalDays):f0}";
        public static string ToMinutesString(TimeSpan avgTTR) => $"{(avgTTR == TimeSpan.Zero ? "" : avgTTR.TotalMinutes):f0}";
        public static string ToHoursOrMinutesString(TimeSpan timeSpend) => $"{(timeSpend == TimeSpan.Zero ? "" : timeSpend.TotalMinutes / 60 < 1 ? $"{timeSpend.TotalMinutes / 60:f1}" : $"{timeSpend.TotalMinutes / 60:f0}")}";
        public static string ToEnumString<T>(T type)
        {
            var enumType = typeof(T);
            var name = Enum.GetName(enumType, type);
            var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
            return enumMemberAttribute.Value;
        }
        public static string ToUpperFirstLetterString(string value) => value.Length switch
        {
            0 => value,
            1 => char.ToUpper(value[0]).ToString(),
            _ => char.ToUpper(value[0]) + value.Substring(1)
        };
        public static string ToEmptyIfZeroString(int value) => value switch
        {
            0 => "",
            _ => value.ToString()
        };
    }
}

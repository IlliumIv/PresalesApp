using Microsoft.JSInterop;
using PresalesMonitor.Shared;
using System.Runtime.Serialization;
using System.Text;

namespace PresalesMonitor.Client.Web.Helpers
{
    public static class Helpers
    {
        public static string CurMonthName => $"{DateTime.Now:MMMM}";
        public static string ToMinMaxFormatString(DateOnly? value) => $"{value:yyyy-MM-dd}";
        public static string ToCurrencyString(decimal value) => $"{(value > 0 ? value : ""):C}";
        public static string ToPercentString(double value, int digits = 0) => $"{(value == 0 ? "" : value.ToString($"P{digits}"))}";
        public static string ToDaysString(TimeSpan avgTTW) => $"{(avgTTW == TimeSpan.Zero ? "" : avgTTW.TotalDays):f0}";
        public static string ToMinutesString(TimeSpan avgTTR) => $"{(avgTTR == TimeSpan.Zero ? "" : avgTTR.TotalMinutes):f0}";
        public static string ToHoursString(TimeSpan timeSpend) => $"{(timeSpend == TimeSpan.Zero ? "" : timeSpend.TotalHours):f1}";
        public static string ToEnumString<T>(T type) where T : Enum
        {
            var enumType = typeof(T);
            var name = Enum.GetName(enumType, type);
            if (name == null) return "";
            var fieldName = enumType.GetField(name);
            if (fieldName == null) return "";
            EnumMemberAttribute enumMemberAttribute = ((EnumMemberAttribute[])fieldName.GetCustomAttributes(typeof(EnumMemberAttribute), true)).Single();
            return enumMemberAttribute.Value ?? "";
        }
        public static string ToUpperFirstLetterString(string value) => value.Length switch
        {
            0 => value,
            1 => char.ToUpper(value[0]).ToString(),
            _ => char.ToUpper(value[0]) + value[1..]
        };
        public static string ToEmptyIfZeroString(int value) => value switch
        {
            0 => "",
            _ => value.ToString()
        };
        private async static Task SaveAs(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        }
        public async static void DownloadFile(Kpi kpi, IJSRuntime js, string presaleName, string monthName)
        {
            if (kpi == null) return;

            string text = $"№;Контрагент;Номер;Дата;Сумма по счёту;Себестоимость;Прибыль;Процент;Премия;Проекты\n";
            int i = 0;
            foreach (var inv in kpi.Invoices)
            {
                i++;
                text += $"{i};{inv.Counterpart};{inv.Number};{inv.Date.ToDateTime().ToLocalTime()};" +
                $"{(decimal)inv.Amount};{(decimal)inv.Cost};" +
                $"{(decimal)inv.SalesAmount};{(inv.Percent > 0 ? inv.Percent * 100 : "")};" +
                $"{((decimal)inv.Profit > 0 ? (decimal)inv.Profit : "")};";
                foreach (var project in inv.ProjectsFound)
                {
                    text += $"{project.Number}, ";
                }
                if (inv.ProjectsFound.Any()) text = text[..^2];
                text += "\n";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] result = Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
            UTF8Encoding encoder = new(true);
            text = encoder.GetString(result);
            await SaveAs(js, $"Отчёт KPI за {ToUpperFirstLetterString(monthName)}, {presaleName}.csv", Encoding.UTF8.GetBytes(text));
        }
    }
}

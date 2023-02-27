using Google.Protobuf.WellKnownTypes;
using Microsoft.JSInterop;
using PresalesMonitor.Shared;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesMonitor.Client.Web.Helpers
{
    public static class Helpers
    {
        public static string CurMonthName => $"{DateTime.Now:MMMM}";
        public static string ToMinMaxFormatString(DateOnly? value) => $"{value:yyyy-MM-dd}";
        public static string ToCurrencyString(decimal value, bool allowNegatives = false) => $"{(allowNegatives ? value : value > 0 ? value : ""):C}";
        public static string ToPercentString(double value, int digits = 0) => $"{(value == 0 ? "" : value.ToString($"P{digits}"))}";
        public static string ToDaysString(TimeSpan avgTTW) => $"{(avgTTW == TimeSpan.Zero ? "" : avgTTW.TotalDays):f0}";
        public static string ToMinutesString(TimeSpan avgTTR) => $"{(avgTTR == TimeSpan.Zero ? "" : avgTTR.TotalMinutes):f0}";
        public static string ToHoursString(TimeSpan timeSpend) => $"{(timeSpend == TimeSpan.Zero ? "" : timeSpend.TotalHours):f1}";
        public static string ToDateString(Timestamp timestamp, string separator) =>
            timestamp.ToDateTime() == DateTime.MinValue ? "" : $"{separator}{timestamp.ToDateTime().ToPresaleTime()}";
        public static string ToOneDateString(Timestamp a, Timestamp b) =>
            $"{(a.ToDateTime() == DateTime.MinValue ? b.ToDateTime().ToPresaleTime() : a.ToDateTime().ToPresaleTime())}";
        public static string ToUpperFirstLetterString(string value) => value.Length switch
        {
            0 => value,
            1 => char.ToUpper(value[0]).ToString(),
            _ => char.ToUpper(value[0]) + value[1..]
        };
        public static string ToEmptyIfZeroString(double value) => value switch
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
        public async static void DownloadFile(Kpi kpi, IJSRuntime js, string presaleName, int month, int year)
        {
            if (kpi == null) return;

            string text = $"№;Контрагент;Номер;Дата;Сумма по счёту;Себестоимость;Прибыль;Процент;Премия;Проекты\n";
            int i = 0;
            foreach (var inv in kpi.Invoices)
            {
                i++;
                text += $"{i};{inv.Counterpart};{inv.Number};{inv.Date.ToDateTime().ToPresaleTime()};" +
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
            await SaveAs(js, $"Отчёт KPI за {ToUpperFirstLetterString(DateTimeFormatInfo.CurrentInfo.GetMonthName(month))} {year}, {presaleName}.csv", Encoding.UTF8.GetBytes(text));
        }
        public static string GetName(this Department department) => department switch
        {
            Department.None => "Без направления",
            Department.Russian => "Россия и СНГ",
            Department.International => "Международное",
            Department.Any => "Любое заданное",
            _ => throw new NotImplementedException()
        };
        public static string GetName(this Position position) => position switch
        {
            Position.None => "Без должности",
            Position.Account => "Аккаунт",
            Position.Engineer => "Инженер",
            Position.Director => "Руководитель",
            Position.Any => "Любая заданная",
            _ => throw new NotImplementedException()
        };
        // TODO: Реализовать поддержку рабочего времени пресейла.
        public static DateTime ToPresaleTime(this DateTime dateTime) => dateTime.ToLocalTime();
    }
}

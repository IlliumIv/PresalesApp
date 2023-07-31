using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using PresalesApp.Web.Shared;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Action = PresalesApp.Web.Shared.Action;

namespace PresalesApp.Web.Client.Helpers
{
    public static partial class Helpers
    {
        public static string CurMonthName => $"{DateTime.Now:MMMM}";

        [GeneratedRegex("\\s+")]
        private static partial Regex DeleteMultipleSpaces();

        public static string GetFirstAndLastName(this string name) => string.Join(" ", DeleteMultipleSpaces().Replace(name, " ").Split().Take(2));

        public static string ToMinMaxFormatString(DateOnly? value) => $"{value:yyyy-MM-dd}";

        public static string ToCurrencyString(this decimal value, bool allowNegatives = false, CultureInfo? cultureInfo = null)
        {
            cultureInfo ??= new CultureInfo("ru-RU");
            var numberFormat = cultureInfo.NumberFormat;

            string pattern = (value >= decimal.Zero) switch
            {
                true => numberFormat.CurrencyPositivePattern switch
                {
                    0 => "{0}{1}",
                    1 => "{1}{0}",
                    2 => "{0} {1}",
                    3 => "{1} {0}",
                    _ => throw new NotImplementedException()
                },
                false => allowNegatives switch
                {
                    true => numberFormat.CurrencyNegativePattern switch
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

            return string.Format(cultureInfo, pattern, numberFormat.CurrencySymbol, $"{value:N2}");
        }

        public static string ToPercentString(double value, int digits = 0) =>
            value == 0 ? "" : string.Format($"{{0:P{digits}}}", value);

        public static string ToDaysString(TimeSpan avgTTW) => avgTTW == TimeSpan.Zero ? "" : $"{avgTTW.TotalDays:f0}";

        public static string ToMinutesString(TimeSpan avgTTR) => avgTTR == TimeSpan.Zero ? "" : $"{avgTTR.TotalMinutes:f0}";

        public static string ToHoursString(TimeSpan timeSpend) => timeSpend == TimeSpan.Zero ? "" : $"{timeSpend.TotalHours:f1}";

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
                "SaveAsFile",
                filename,
                Convert.ToBase64String(data));
        }

        public async static Task Download(this Kpi? kpi, IJSRuntime js, string presaleName, int month, int year, IStringLocalizer<App> localization)
        {
            if (kpi == null) return;

            string text = $"{localization["OrderNumberText"]};" +
                $"{localization["CounterpartText"]};" +
                $"{localization["InvoiceNumberText"]};" +
                $"{localization["InvoiceDateText"]};" +
                $"{localization["InvoiceAmountText"]};" +
                $"{localization["InvoiceCostPriceText"]};" +
                $"{localization["InvoiceProfitText"]};" +
                $"{localization["PresalePercentText"]};" +
                $"{localization["PresaleProfitText"]};" +
                $"{localization["ProjectsText"]}\n";

            int i = 0;
            foreach (var invoice in kpi.Invoices)
            {
                i++;
                text += $"{i};{invoice.Counterpart};{invoice.Number};{invoice.Date.ToDateTime().ToPresaleTime()};" +
                $"{(decimal)invoice.Amount};{(decimal)invoice.Cost};" +
                $"{(decimal)invoice.SalesAmount};{(invoice.Percent > 0 ? invoice.Percent * 100 : "")};" +
                $"{((decimal)invoice.Profit > 0 ? (decimal)invoice.Profit : "")};";
                foreach (var project in invoice.ProjectsFound)
                {
                    text += $"{project.Number}, ";
                }
                if (invoice.ProjectsFound.Any()) text = text[..^2];
                text += "\n";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] result = Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
            UTF8Encoding encoder = new(true);
            text = encoder.GetString(result);
            await SaveAs(js, $"{(MarkupString)localization["KpiReportFileName",
                        ToUpperFirstLetterString(DateTimeFormatInfo.CurrentInfo.GetMonthName(month)),
                        year, presaleName.GetFirstAndLastName()].Value}.csv", Encoding.UTF8.GetBytes(text));
        }

        public async static Task Download(this UnpaidProjects? projects, IJSRuntime js, IStringLocalizer<App> localization)
        {
            if (projects == null) return;

            string text = $"{localization["ProjectNumberText"]};" +
                $"{localization["ProjectNameText"]};" +
                $"{localization["PresaleNameText"]};" +
                $"{localization["ProjectStatusText"]};" +
                $"{localization["ApprovalBySalesDirectorDateText"]} ;" +
                $"{localization["ApprovalByTechDirectorDateText"]} ;" +
                $"{localization["PresaleStartDateText"]} ;" +
                $"{localization["ProjectCloseDateText"]} ;" +
                $"{localization["ProjectInvoicesCountText"]}\n";

            foreach(var project in projects.Projects)
            {
                text += $"{project.Number};{project.Name};{project.Presale?.Name.GetFirstAndLastName()};{project.Status.GetLocalizedName(localization)};" +
                    $"{project.ApprovalBySalesDirectorAt.ToDateTime().ToPresaleTime()};" +
                    $"{project.ApprovalByTechDirectorAt.ToDateTime().ToPresaleTime()};" +
                    $"{project.PresaleStartAt.ToDateTime().ToPresaleTime()};" +
                    $"{project.ClosedAt.ToDateTime().ToPresaleTime()};" +
                    $"{project.Invoices.Count}\n";
            }

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] result = Encoding.UTF8.GetPreamble().Concat(bytes).ToArray();
            UTF8Encoding encoder = new(true);
            text = encoder.GetString(result);
            await SaveAs(js, $"{(MarkupString)localization["UnpaidReportFileName", DateTime.Now].Value}.csv", Encoding.UTF8.GetBytes(text));
        }

        public static string GetLocalizedName(this Department department, IStringLocalizer<App> localization) => department switch
        {
            Department.None => localization["DepartmentNoneText"],
            Department.Russian => localization["DepartmentRussianText"],
            Department.International => localization["DepartmentInternationalText"],
            Department.Any => localization["DepartmentAnyText"],
            _ => throw new NotImplementedException()
        };

        public static string GetLocalizedName(this FunnelStage stage, IStringLocalizer<App> localization) => stage switch
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

        public static string GetLocalizedName(this Position position, IStringLocalizer<App> localization) => position switch
        {
            Position.None => localization["PositionNoneText"],
            Position.Account => localization["PositionAccountText"],
            Position.Engineer => localization["PositionEngineerText"],
            Position.Director => localization["PositionDirectorText"],
            Position.Any => localization["PositionAnyText"],
            _ => throw new NotImplementedException()
        };

        public static string GetLocalizedName(this ProjectStatus status, IStringLocalizer<App> localization) => status switch
        {
            ProjectStatus.Unknown => localization["ProjectStatusUnknownText"],
            ProjectStatus.WorkInProgress => localization["ProjectStatusWorkInProgressText"],
            ProjectStatus.Won => localization["ProjectStatusWonText"],
            ProjectStatus.Loss => localization["ProjectStatusLossText"],
            _ => throw new NotImplementedException()
        };

        // TODO: Реализовать поддержку рабочего времени пресейла.
        public static DateTime ToPresaleTime(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue) return DateTime.MinValue;
            return dateTime.ToLocalTime();
        }

        public static string Format(this Action action, IStringLocalizer<App> localization) =>
            $"{action.ProjectNumber} [{action.Type}" +
            $"{ToDateString(action.Date, ": ")}" +
            $" ({action.Timespend.ToTimeSpan().TotalMinutes})], \"{action.Description}\"" +
            (action.SalesFunnel ? $" ({localization["ActionSalesFunnelMarkText"]})." : ".");

        public static string Format(this Project project, IStringLocalizer<App> localization) =>
            $"{project.Number} [{project.Status.GetLocalizedName(localization)}" +
            $"{ToDateString(project.ClosedAt, ": ")}" +
            $"], \"{project.Name}\"";

        public static string SetColor(Invoice invoice) =>
            invoice.ProjectsIgnored.Any() || invoice.ActionsIgnored.Any() || (decimal)invoice.Profit == 0 ? "red" : "inherit";
    }
}

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Shared;
using Blazorise.Charts;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using System.Globalization;
using PresalesApp.Web.Client.Enums;

namespace PresalesApp.Web.Client.Pages
{
    partial class PresalesNewYearDashboard
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        #region UriQuery
        private const string q_keyword = "Keyword";
        [SupplyParameterFromQuery(Name = q_keyword)] public string? Keyword { get; set; }

        private const string q_start = "Start";
        [SupplyParameterFromQuery(Name = q_start)] public string? Start { get; set; }

        private const string q_end = "End";
        [SupplyParameterFromQuery(Name = q_end)] public string? End { get; set; }

        private const string q_department = "Department";
        [SupplyParameterFromQuery(Name = q_department)] public string? DepartmentType { get; set; }

        private Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = period.Start.ToString(Helper.UriDateTimeFormat),
            [q_end] = period.End.ToString(Helper.UriDateTimeFormat),
            [q_keyword] = image_keyword,
            [q_department] = _department.ToString(),
        };
        #endregion

        private Department _department = Department.Russian;
        private readonly Helpers.Period period = new(new(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc), PeriodType.Quarter);
        private static ImageResponse img;
        private string image_keyword = "woman";
        private ProfitOverview overview;
        private TimeSpan time_left = new(0, 10, 0);
        private readonly PeriodicTimer periodic_timer = new(TimeSpan.FromSeconds(1));

        private bool IsRealisticPlanDone => (overview?.Profit.Values.Sum(amount => amount) ?? 0) > 120_000_000;
        private decimal GetCurrentDayProfit() =>
            overview?.Profit?.FirstOrDefault(per => per.Key == DateTime.Now.StartOfDay()
            .ToString(CultureInfo.InvariantCulture)).Value;
        private static string GetHeaderCellStyling() => "text-align: right";
        private static string GetProgressPercentString(DecimalValue? a, DecimalValue? b) => $"{(decimal)a / (decimal)b * 100:N0}%";

        protected override void OnInitialized()
        {
            Helper.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref period.Start);
            Helper.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref period.End);
            Helper.SetFromQueryOrStorage(value: Keyword, query: q_keyword, uri: Navigation.Uri, storage: Storage, param: ref image_keyword);
            Helper.SetFromQueryOrStorage(value: DepartmentType, query: q_department, uri: Navigation.Uri, storage: Storage, param: ref _department);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            RunTimer();
        }

        private async void RunTimer()
        {
            int count = 600;
            var second = new TimeSpan(0, 0, 1);
            while (await periodic_timer.WaitForNextTickAsync())
            {
                time_left = time_left.Subtract(second);
                if (count > 600)
                {
                    count = 0;
                    await UpdateData();
                    await UpdateImage();
                    await RedrawChart();
                    time_left = new(0, 10, 0);
                }
                count++;
                StateHasChanged();
            }
        }

        private async Task UpdateData()
        {
            try
            {
                overview = await AppApi.GetProfitOverviewAsync(new OverviewRequest
                {
                    Period = period.Translate(),
                    Department = _department,
                    Position = Position.Any
                });
            }
            catch
            {
                await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
                return;
            }
        }
        
        private async Task UpdateImage() => img = await AppApi.GetImageAsync(new ImageRequest
        {
            Keyword = image_keyword,
            Orientation = ImageOrientation.Portrait
        });

        private async void OnManuallyImageUpdate(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{q_keyword}", image_keyword);
                Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
                await UpdateImage();
            }
        }

        #region Charts
        private LineChart<decimal> line_chart;
        private PieChart<int> invoices_chart;

        private static string GetChartOptions() => "{\"aspectRatio\":2.43875, \"plugins\":{\"legend\":{\"display\": false}}}";
        private static string GetInvoicesChartOptions() => "{\"cutout\":\"80%\",\"animation\":{\"animateScale\": true}}";

        private static LineChartDataset<decimal> GetChartDataset(List<decimal> data) => new()
        {
            Label = "Сумма",
            Data = data,
            BackgroundColor = GetColors(0.3f, max: 1),
            BorderColor = GetColors(0.8f, max: 1),
            Fill = true,
            PointRadius = 1,
        };

        private static LineChartDataset<decimal> GetRealisticPlanDataset(List<decimal> data) => new()
        {
            Label = "Реалистичный план",
            Data = data,
            BorderColor = GetColors(0.8f),
            PointRadius = 0,
        };

        private static LineChartDataset<decimal> GetAmbitiousPlanDataset(List<decimal> data) => new()
        {
            Label = "Амбициозный план",
            Data = data,
            BorderColor = GetColors(0.8f),
            PointRadius = 0,
        };

        private static LineChartDataset<decimal> GetMinPointDataset() => new()
        {
            Label = "min point",
            Data = [155_000_000],
            PointRadius = 0,
        };

        #region Colors
        private readonly static (byte R, byte G, byte B)[] colors = [ (12, 90, 74), (5, 47, 91), (165, 14, 130), (232, 125, 55),
            (106, 158, 31), (20, 150, 124), (99, 8, 78), (198, 35, 36), (3, 28, 58) ];

        private static List<string> GetColors(float alfa, int max = int.MaxValue)
        {
            var r = new List<string>();

            for (int i = 0; i < colors.Length && i < max; i++)
            {
                r.Add(ChartColor.FromRgba(colors[i].R, colors[i].G, colors[i].B, alfa));
            }

            return r;
        }
        #endregion

        private async Task RedrawChart()
        {
            if (overview == null) return;
            await line_chart.Clear();
            await invoices_chart.Clear();

            var labels = new List<string>();
            var profit = new List<decimal>();
            var realistic_plan = new List<decimal>();
            var ambitious_plan = new List<decimal>();
            decimal amount = 0;

            foreach ((var date, var day_profit) in overview.Profit)
            {
                var dt = DateTime.Parse(date, CultureInfo.InvariantCulture);
                if (dt > DateTime.Now) break;
                amount += day_profit;
                labels.Add(dt.GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization));
                profit.Add(amount);
                realistic_plan.Add(120_000_000);
                ambitious_plan.Add(150_000_000);
            }

            var invoices = new List<int>();
            foreach (var presale in overview.Presales.OrderByDescending(p => p.Statistics.Profit))
                invoices.Add(presale.Statistics.InvoicesShipped);

            await line_chart.AddLabelsDatasetsAndUpdate(labels, GetChartDataset(profit), GetRealisticPlanDataset(realistic_plan), GetAmbitiousPlanDataset(ambitious_plan), GetMinPointDataset());
            await invoices_chart.AddDatasetsAndUpdate(new PieChartDataset<int>()
            {
                Data = invoices,
                BackgroundColor = GetColors(0.3f),
                BorderColor = GetColors(0.8f),
            });
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }
    }
}

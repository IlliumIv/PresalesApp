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

        private static ImageResponse img;
        private string image_keyword = "woman";

        private string GetPlanName() => (overview?.Profit.Values.Sum(amount => amount) ?? 0) > 120_000_000 ? "Амбициозный" : "Реалистичный";

        private ProfitOverview overview;

        private TimeSpan time_left = new(0, 10, 0);
        private readonly PeriodicTimer periodic_timer = new(TimeSpan.FromSeconds(1));

        private decimal GetCurrentDayProfit() => overview?.Profit?.FirstOrDefault(per => per.Key == DateTime.Now.StartOfDay().ToString(CultureInfo.InvariantCulture)).Value;

        private async void OnManuallyImageUpdate(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await UpdateImage();
            }
        }

        private async Task UpdateImage() => img = await AppApi.GetImageAsync(new ImageRequest
        {
            Keyword = image_keyword,
            Orientation = ImageOrientation.Portrait
        });

        protected override void OnInitialized()
        {
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
                    Period = new Helpers.Period(new DateTime(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc), Enums.PeriodType.Quarter).Translate(),
                    Department = Department.Any,
                    Position = Position.Any
                });
            }
            catch
            {
                await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
                return;
            }
        }

        private static string GetProgressPercentString(DecimalValue? a, DecimalValue? b) => $"{(decimal)a / (decimal)b * 100:N0}%";

        private static string GetHeaderCellStyling() => "text-align: right";

        #region Chart
        // private static readonly double aspectRatio = 2.43875;
        private LineChart<decimal> lineChart;
        private readonly List<string> backgroundColors = [ChartColor.FromRgba(12, 90, 74, 0.2f), ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f)];
        private readonly List<string> borderColors = [ChartColor.FromRgba(12, 90, 74, 0.8f), ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f)];
        private static readonly List<double> SmallPlanLine = [120, 120, 120, 120, 120, 120];
        private static readonly List<double> BigPlanLine = [150, 150, 150, 150, 150, 150];
        private static string GetChartOptions() => "{\"aspectRatio\":2.43875, \"plugins\":{\"legend\":{\"display\": false}}}";

        private async Task RedrawChart()
        {
            if (overview == null) return;
            await lineChart.Clear();

            var labels = new List<string>();
            var profit = new List<decimal>();
            var realistic_plan = new List<decimal>();
            var ambitious_plan = new List<decimal>();
            // var backgroud_colors = new List<string>();
            // var border_colors = new List<string>();
            decimal amount = 0;

            foreach ((var date, var day_profit) in overview.Profit)
            {
                amount += day_profit;
                labels.Add(DateTime.Parse(date, CultureInfo.InvariantCulture).GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization));
                profit.Add(amount);
                realistic_plan.Add(120_000_000);
                ambitious_plan.Add(150_000_000);
            }

            await lineChart.AddLabelsDatasetsAndUpdate(labels, GetChartDataset(profit), GetRealisticPlanDataset(realistic_plan), GetAmbitiousPlanDataset(ambitious_plan), GetMinPointDataset());
        }

        private LineChartDataset<decimal> GetChartDataset(List<decimal> data) => new()
        {
            Label = "profit",
            Data = data,
            BackgroundColor = backgroundColors.Take(1).ToList(),
            BorderColor = borderColors.Take(1).ToList(),
            Fill = true,
            PointRadius = 1,
            // CubicInterpolationMode = "monotone",
        };

        private LineChartDataset<decimal> GetRealisticPlanDataset(List<decimal> data) => new()
        {
            Label = "Реалистичный план",
            Data = data,
            BorderColor = borderColors,
            PointRadius = 0,
            BorderDash = [15, 5],
        };

        private LineChartDataset<decimal> GetAmbitiousPlanDataset(List<decimal> data) => new()
        {
            Label = "Амбициозный план",
            Data = data,
            BorderColor = borderColors,
            PointRadius = 0,
            BorderDash = [15, 5],
        };

        private static LineChartDataset<decimal> GetMinPointDataset() => new()
        {
            Label = "min point",
            Data = [155_000_000],
            PointRadius = 0,
        };
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }
    }
}

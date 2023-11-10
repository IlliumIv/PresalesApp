﻿using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Client.Enums;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using System.Globalization;

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

        private const string q_keyword_type = "KeywordType";
        [SupplyParameterFromQuery(Name = q_keyword_type)] public string? KeywordType { get; set; }

        private Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = period.Start.ToString(Helper.UriDateTimeFormat),
            [q_end] = period.End.ToString(Helper.UriDateTimeFormat),
            [q_keyword] = image_keyword,
            [q_department] = department.ToString(),
            [q_keyword_type] = keyword_type.ToString(),
        };
        #endregion

        private static decimal day_profit = 0;
        private static List<Presale> sorted_presales;
        private static Department department = Department.Russian;
        private static ImageKeywordType keyword_type = ImageKeywordType.Query;
        private static readonly Helpers.Period period = new(new(2023, 10, 1, 0, 0, 0, DateTimeKind.Utc), PeriodType.Quarter);
        private static ImageResponse img;
        private string image_keyword = "woman";
        private static ProfitOverview overview;
        private static PeriodicTimer periodic_timer = new(TimeSpan.FromSeconds(1));

        private static bool IsRealisticPlanDone() => (overview?.Profit.Values.Sum(amount => amount) ?? 0) > 120_000_000;
        private static string GetHeaderCellStyling() => "text-align: right";
        private static string GetProgressPercentString(DecimalValue? a, DecimalValue? b) => $"{(decimal)a / (decimal)b * 100:N0}%";

        protected override void OnInitialized()
        {
            Helper.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref period.Start);
            Helper.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref period.End);
            Helper.SetFromQueryOrStorage(value: Keyword, query: q_keyword, uri: Navigation.Uri, storage: Storage, param: ref image_keyword);
            Helper.SetFromQueryOrStorage(value: DepartmentType, query: q_department, uri: Navigation.Uri, storage: Storage, param: ref department);
            Helper.SetFromQueryOrStorage(value: KeywordType, query: q_keyword_type, uri: Navigation.Uri, storage: Storage, param: ref keyword_type);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            RunTimer();
        }

        private async void RunTimer()
        {
            while (await periodic_timer.WaitForNextTickAsync())
            {
                Console.WriteLine(DateTime.Now);
                await UpdateData();
                await UpdateImage();
                await RedrawChart();
                StateHasChanged();
                periodic_timer = new(TimeSpan.FromMinutes(10));
            }
        }

        private async Task UpdateData()
        {
            try
            {
                overview = await AppApi.GetProfitOverviewAsync(new OverviewRequest
                {
                    Period = period.Translate(),
                    Department = department,
                    Position = Position.Any
                });

                sorted_presales = [.. overview.Presales.OrderByDescending(p => p.Statistics.Profit)];
                day_profit = overview.Profit
                    .FirstOrDefault(p => p.Key == DateTime.Now.Date.ToUniversalTime()
                    .ToString(CultureInfo.InvariantCulture)).Value ?? 0;
            }
            catch
            {
                await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
                return;
            }
        }

        private async Task UpdateImage()
        {
            try
            {
                img = await AppApi.GetImageAsync(new ImageRequest
                {
                    Keyword = image_keyword,
                    Orientation = ImageOrientation.Portrait,
                    KeywordType = keyword_type
                });
            }
            catch
            {
                await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
                return;
            }
        }

        private async void OnManuallyImageUpdate(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{q_keyword}", image_keyword);
                Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
                await UpdateImage();
            }
        }

        private void OnImageKeywordTypeChange()
        {
            keyword_type = keyword_type switch
            {
                ImageKeywordType.Query => ImageKeywordType.Collections,
                ImageKeywordType.Collections => ImageKeywordType.Query,
                _ => throw new NotImplementedException()
            };
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_keyword_type}", keyword_type);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
        }

        #region Charts
        private static LineChart<decimal> line_chart;

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
        private static readonly (byte R, byte G, byte B)[] colors = [(12, 90, 74),
            (5, 47, 91),
            (165, 14, 130),
            (232, 125, 55),
            (106, 158, 31),
            (20, 150, 124),
            (99, 8, 78),
            (198, 35, 36),
            (3, 28, 58)];

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

            var labels = new List<string>();
            var profit = new List<decimal>();
            var realistic_plan = new List<decimal>();
            var ambitious_plan = new List<decimal>();
            decimal amount = 0;

            foreach ((var date, var day_profit) in overview.Profit)
            {
                var dt = DateTime.Parse(date, CultureInfo.InvariantCulture).ToLocalTime();
                if (dt > DateTime.Now) break;
                amount += day_profit;
                labels.Add(dt.GetLocalizedDateNameByPeriodType(PeriodType.Day, Localization));
                profit.Add(amount);
                realistic_plan.Add(120_000_000);
                ambitious_plan.Add(150_000_000);
            }

            var invoices = new List<int>();
            foreach (var presale in overview.Presales.OrderByDescending(p => p.Statistics.Profit))
            {
                invoices.Add(presale.Statistics.InvoicesShipped);
            }

            await line_chart.AddLabelsDatasetsAndUpdate(labels: labels,
                GetChartDataset(profit),
                GetRealisticPlanDataset(realistic_plan),
                GetAmbitiousPlanDataset(ambitious_plan),
                GetMinPointDataset());
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }
    }
}

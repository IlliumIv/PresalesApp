﻿using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Department = PresalesApp.Web.Shared.Department;
using Position = PresalesApp.Web.Shared.Position;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages
{
    partial class PresalesDashboard
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        private TimeSpan time_left = new(0, 10, 0);

        private MonthProfitOverview overview;

        private PieChart<decimal> chart;

        private static ImageResponse img;

        private string image_keyword = "girl";

        #region UriQuery
        private const string q_keyword = "Keyword";
        [SupplyParameterFromQuery(Name = q_keyword)] public string? Keyword { get; set; }

        private const string q_start = "Start";
        [SupplyParameterFromQuery(Name = q_start)] public string? Start { get; set; }

        private const string q_end = "End";
        [SupplyParameterFromQuery(Name = q_end)] public string? End { get; set; }

        private Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = period.Start.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_end] = period.End.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_keyword] = image_keyword,
        };
        #endregion

        protected override void OnInitialized()
        {
            Helpers.Helpers.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref period.Start);
            Helpers.Helpers.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref period.End);
            Helpers.Helpers.SetFromQueryOrStorage(value: Keyword, query: q_keyword, uri: Navigation.Uri, storage: Storage, param: ref image_keyword);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            RunTimer();
        }

        private readonly PeriodicTimer periodic_timer = new(TimeSpan.FromSeconds(1));
        private static DateTime GetFirstDay() => new(DateTime.Now.Year, DateTime.Now.Month, 1);
        private static DateTime GetLastDay() => GetFirstDay().AddMonths(1).AddSeconds(-1);
        private readonly Period period = new(GetFirstDay(), GetLastDay());
        private static string GetChartOptions() => "{\"cutout\":\"30%\",\"animation\":{\"animateScale\": true},\"plugins\":{\"tooltip\":{\"enabled\": false}}}";

        private readonly (byte R, byte G, byte B)[] colors = [
            (5, 47, 91), (165, 14, 130), (12, 90, 74), (232, 125, 55), (106, 158, 31),
            (20, 150, 124), (99, 8, 78), (198, 35, 36), (3, 28, 58) ];

        private readonly float color_alfa = 0.5f;

        private async void RunTimer()
        {
            int count = 0;
            var second = new TimeSpan(0, 0, 1);
            while (await periodic_timer.WaitForNextTickAsync())
            {
                time_left = time_left.Subtract(second);
                if (count > 600)
                {
                    count = 0;
                    period.Start = GetFirstDay();
                    period.End = GetLastDay();
                    await UpdateData(period);
                    await UpdateImage();
                    await RedrawChart();
                    time_left = new(0, 10, 0);
                }
                count++;
                StateHasChanged();
            }
        }

        private async Task UpdateData(Period period)
        {
            try
            {
                overview = await AppApi.GetMonthProfitOverviewAsync(new OverviewRequest
                {
                    Period = period.Translate(),
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

        private async Task UpdateImage() => img = await AppApi.GetImageAsync(new ImageRequest
        {
            Keyword = image_keyword,
            Orientation = ImageOrientation.Portrait
        });

        private async Task RedrawChart()
        {
            if (overview == null) return;

            await chart.Clear();
            var dataset = new List<decimal>();
            var backgroud_colors = new List<string>();
            var border_colors = new List<string>();

            foreach (var item in overview.Presales.Select((presale, index) => new { index, presale }))
            {
                if ((decimal)item.presale.Statistics.Profit > 0)
                {
                    dataset.Add(item.presale.Statistics.Profit);
                    backgroud_colors.Add(ChartColor.FromRgba(colors[item.index].R, colors[item.index].G, colors[item.index].B, color_alfa));
                    border_colors.Add(ChartColor.FromRgba(colors[item.index].R, colors[item.index].G, colors[item.index].B, 1));
                };
            };

            if ((decimal)overview.Left > 0)
            {
                dataset.Add(overview.Left);
                backgroud_colors.Add(ChartColor.FromRgba(240, 240, 240, 0.5f));
                border_colors.Add(ChartColor.FromRgba(240, 240, 240, 1));
            }

            await chart.AddDatasetsAndUpdate(new PieChartDataset<decimal>()
            {
                Data = dataset,
                BackgroundColor = backgroud_colors,
                BorderColor = border_colors
            });
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UpdateData(period);
                await UpdateImage();
                await RedrawChart();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }
    }
}
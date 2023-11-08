using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages
{
    partial class SalesDashboard
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        #region UriQuery
        private const string q_prev_start = "pStart";
        [SupplyParameterFromQuery(Name = q_prev_start)] public string? PrevStart { get; set; }

        private const string q_prev_end = "pEnd";
        [SupplyParameterFromQuery(Name = q_prev_end)] public string? PrevEnd { get; set; }

        private const string q_prev_period_type = "pPeriod";
        [SupplyParameterFromQuery(Name = q_prev_period_type)] public string? PrevPeriodType { get; set; }

        private const string q_curr_start = "cStart";
        [SupplyParameterFromQuery(Name = q_curr_start)] public string? CurrStart { get; set; }

        private const string q_curr_end = "cEnd";
        [SupplyParameterFromQuery(Name = q_curr_end)] public string? CurrEnd { get; set; }

        private const string q_curr_period_type = "cPeriod";
        [SupplyParameterFromQuery(Name = q_curr_period_type)] public string? CurrPeriodType { get; set; }

        private Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_prev_start] = Previous.Start.ToString(Helper.UriDateTimeFormat),
            [q_prev_end] = Previous.End.ToString(Helper.UriDateTimeFormat),
            [q_prev_period_type] = Previous.Type.ToString(),
            [q_curr_start] = Current.Start.ToString(Helper.UriDateTimeFormat),
            [q_curr_end] = Current.End.ToString(Helper.UriDateTimeFormat),
            [q_curr_period_type] = Current.Type.ToString(),
        };
        #endregion

        private string title_pin = "Закрепить";
        private string class_hide = "hide";
        private string class_rotatePin = "rotatePin";
        private SalesOverview? overview;
        private List<string> dataset_colors = [];
        private List<string> border_colors = [];

        private static DateTime GetFirstDay() => new(DateTime.Now.Year, ((DateTime.Now.Month - 1) / 3 + 1) * 3 - 2, 1);
        private Period Current = new(GetFirstDay(), Enums.PeriodType.Quarter);
        private Period Previous = new(GetFirstDay().AddYears(-1), Enums.PeriodType.Quarter);

        private PieChart<decimal> profitChart;

        private async Task OnPeriodChanged()
        {
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_prev_start}", Previous.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_prev_end}", Previous.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_prev_period_type}", Previous.Type);

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_curr_start}", Current.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_curr_end}", Current.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_curr_period_type}", Current.Type);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await HandleRedrawChart();
        }

        private void PinParams()
        {
            if (class_hide.Length > 0)
            {
                class_hide = "";
                title_pin = "Открепить";
                class_rotatePin = "";
            }
            else
            {
                class_hide = "hide";
                title_pin = "Закрепить";
                class_rotatePin = "rotatePin";
            }
        }

        private readonly PeriodicTimer periodic_timer = new(TimeSpan.FromMinutes(10));

        private async Task UpdateData()
        {
            try
            {
                overview = await AppApi.GetSalesOverviewAsync(new SalesOverviewRequest
                {
                    Previous = Previous.Translate(),
                    Current = Current.Translate()
                });
            }
            catch
            {
                await GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value);
                StateHasChanged();
                return;
            }
        }

        // private async Task UpdateImage() => _img = await PresalesClient.GetImageAsync(new ImageRequest { Keyword = "happy new year", Orientation = ImageOrientation.Landscape });

        protected override void OnInitialized()
        {
            Helper.SetFromQueryOrStorage(value: PrevStart, query: q_prev_start, uri: Navigation.Uri, storage: Storage, param: ref Previous.Start);
            Helper.SetFromQueryOrStorage(value: PrevEnd, query: q_prev_end, uri: Navigation.Uri, storage: Storage, param: ref Previous.End);
            Helper.SetFromQueryOrStorage(value: PrevPeriodType, query: q_prev_period_type, uri: Navigation.Uri, storage: Storage, param: ref Previous.Type);

            Helper.SetFromQueryOrStorage(value: CurrStart, query: q_curr_start, uri: Navigation.Uri, storage: Storage, param: ref Current.Start);
            Helper.SetFromQueryOrStorage(value: CurrEnd, query: q_curr_end, uri: Navigation.Uri, storage: Storage, param: ref Current.End);
            Helper.SetFromQueryOrStorage(value: CurrPeriodType, query: q_curr_period_type, uri: Navigation.Uri, storage: Storage, param: ref Current.Type);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            RunTimer();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }

        private static string GetPieChartOptions() => "{\"animation\":{\"animateScale\": true},\"plugins\":{\"tooltip\":{\"enabled\": false}}}";

        private async void RunTimer()
        {
            while (await periodic_timer.WaitForNextTickAsync())
            {
                await HandleRedrawChart();
            }
        }

        private async Task HandleRedrawChart()
        {
            if (Previous.Start > Previous.End)
            {
                await GlobalMsgHandler.Show("Начало предыдущего периода должно быть меньше окончания!");
                StateHasChanged();
                return;
            }

            if (Current.Start > Current.End)
            {
                await GlobalMsgHandler.Show("Начало текущего периода должно быть меньше окончания!");
                StateHasChanged();
                return;
            }

            await UpdateData();
            if (overview == null) return;

            await profitChart.Clear();
            dataset_colors = [];
            border_colors = [];
            float colors_alfa = 0.5f;

            var dataset = new List<decimal>();
            var rnd = new Random();

            foreach (var manager in overview.CurrentTopSalesManagers)
            {
                dataset.Add(manager.Profit);
                var r = rnd.Next(255);
                var g = rnd.Next(255);
                var b = rnd.Next(255);
                dataset_colors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), colors_alfa));
                border_colors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), 1));
            }

            var sum = overview.CurrentTopSalesManagers.Sum(m => m.Profit);
            if (overview.CurrentActualProfit > sum)
            {
                dataset.Add(overview.CurrentActualProfit - sum);
                var r = rnd.Next(255);
                var g = rnd.Next(255);
                var b = rnd.Next(255);
                dataset_colors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), colors_alfa));
                border_colors.Add(ChartColor.FromRgba(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b), 1));
            }

            if (overview.CurrentSalesTarget > overview.CurrentActualProfit)
            {
                dataset.Add(overview.CurrentSalesTarget - overview.CurrentActualProfit);
                dataset_colors.Add(ChartColor.FromRgba(240, 240, 240, colors_alfa));
                border_colors.Add(ChartColor.FromRgba(240, 240, 240, 1));
            }

            await profitChart.AddDatasetsAndUpdate(new PieChartDataset<decimal>()
            {
                Data = dataset,
                BackgroundColor = dataset_colors,
                BorderColor = border_colors
                // BorderAlign = "center",
                // BorderWidth = 1,
            });

            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await HandleRedrawChart();
            }
        }
    }
}

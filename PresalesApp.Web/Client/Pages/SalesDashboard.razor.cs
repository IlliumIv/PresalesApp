using Microsoft.AspNetCore.Components;
using pax.BlazorChartJs;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using System.Data;
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

        private static DateTime GetFirstDay() => new(DateTime.Now.Year, ((DateTime.Now.Month - 1) / 3 + 1) * 3 - 2, 1);
        private Period Current = new(GetFirstDay(), Enums.PeriodType.Quarter);
        private Period Previous = new(GetFirstDay().AddYears(-1), Enums.PeriodType.Quarter);

        private ChartJsConfig _ProfitChartConfig { get; set; } = ChartHelpers.GenerateChartConfig(ChartType.pie);

        /*
        private readonly ChartJsConfig _ProfitChartConfig = ChartHelpers.GenerateChartConfig(ChartType.pie, null, new()
        {
            Animation = new PieDatasetAnimation() { AnimateScale = true },
            Plugins = new() { Tooltip = new Tooltip() { Enabled = false }}
        });
        */

        private async Task OnPeriodChanged()
        {
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_prev_start}", Previous.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_prev_end}", Previous.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_prev_period_type}", Previous.Type);

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_curr_start}", Current.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_curr_end}", Current.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_curr_period_type}", Current.Type);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await HandleRedrawChartAsync();
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
            catch {  GlobalMsgHandler.Show(Localization["ConnectErrorTryLater", Localization["PWAServerName"]].Value); }
        }

        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("OnInitialized");
            Helper.SetFromQueryOrStorage(value: PrevStart, query: q_prev_start, uri: Navigation.Uri, storage: Storage, param: ref Previous.Start);
            Helper.SetFromQueryOrStorage(value: PrevEnd, query: q_prev_end, uri: Navigation.Uri, storage: Storage, param: ref Previous.End);
            Helper.SetFromQueryOrStorage(value: PrevPeriodType, query: q_prev_period_type, uri: Navigation.Uri, storage: Storage, param: ref Previous.Type);

            Helper.SetFromQueryOrStorage(value: CurrStart, query: q_curr_start, uri: Navigation.Uri, storage: Storage, param: ref Current.Start);
            Helper.SetFromQueryOrStorage(value: CurrEnd, query: q_curr_end, uri: Navigation.Uri, storage: Storage, param: ref Current.End);
            Helper.SetFromQueryOrStorage(value: CurrPeriodType, query: q_curr_period_type, uri: Navigation.Uri, storage: Storage, param: ref Current.Type);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await HandleRedrawChartAsync();
            await base.OnInitializedAsync();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }

        private async void RunTimer()
        {
            while (await periodic_timer.WaitForNextTickAsync())
            {
                await HandleRedrawChartAsync();
            }
        }

        private async Task HandleRedrawChartAsync()
        {
            if (Previous.Start > Previous.End)
            {
                GlobalMsgHandler.Show("Начало предыдущего периода должно быть меньше окончания!");
                return;
            }

            if (Current.Start > Current.End)
            {
                GlobalMsgHandler.Show("Начало текущего периода должно быть меньше окончания!");
                return;
            }

            await UpdateData();

            if (overview == null) return;

            List<decimal> dataset = [];

            foreach (var manager in overview.CurrentTopSalesManagers)
                dataset.Add(manager.Profit);

            var sum = overview.CurrentTopSalesManagers.Sum(m => m.Profit);

            if (overview.CurrentActualProfit > sum)
                dataset.Add(overview.CurrentActualProfit - sum);

            if (overview.CurrentSalesTarget > overview.CurrentActualProfit)
                dataset.Add(overview.CurrentSalesTarget - overview.CurrentActualProfit);

            _ProfitChartConfig.Update(null, ChartHelpers.GetPieDataset(dataset.Cast<object>().ToList(), (240, 240, 240), 0.5f, 1f));

            StateHasChanged();
        }
    }
}

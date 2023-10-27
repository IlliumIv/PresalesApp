using Blazorise.Charts;
using Microsoft.AspNetCore.Components;
using PresalesApp.Bridge1C;
using PresalesApp.Web.Client.Views;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages
{
    partial class TestPage
    {
        private HelloReply? reply;

        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        private int counter = 0;

        #region UriQuery
        private const string queryFrom = "from";
        [SupplyParameterFromQuery(Name = queryFrom)] public string? From { get; set; }

        private const string queryTo = "to";
        [SupplyParameterFromQuery(Name = queryTo)] public string? To { get; set; }

        private const string queryPeriod = "period";
        [SupplyParameterFromQuery(Name = queryPeriod)] public string? PeriodType { get; set; }

        private Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [queryFrom] = Period.Start.ToString(Helpers.Helpers.UriDateTimeFormat),
            [queryTo] = Period.End.ToString(Helpers.Helpers.UriDateTimeFormat),
            [queryPeriod] = Period.Type.ToString(),
        };
        #endregion

        public Period Period = new(DateTime.Now, Enums.PeriodType.Arbitrary);

        private void CleanStorage()
        {
            Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{queryFrom}");
            Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{queryTo}");
            Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{queryPeriod}");
        }

        private async void DoSomething() => await HandleSayHello();

        private void OnPeriodChanged(Period period)
        {
            Period = period;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{queryFrom}", Period.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{queryTo}", Period.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{queryPeriod}", Period.Type);

            counter++;

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
        }

        protected override void OnInitialized()
        {
            Helpers.Helpers.SetFromQueryOrStorage(value: From, query: queryFrom, uri: Navigation.Uri, storage: Storage, param: ref Period.Start);
            Helpers.Helpers.SetFromQueryOrStorage(value: To, query: queryTo, uri: Navigation.Uri, storage: Storage, param: ref Period.End);
            Helpers.Helpers.SetFromQueryOrStorage(value: PeriodType, query: queryPeriod, uri: Navigation.Uri, storage: Storage, param: ref Period.Type);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender) await HandleRedraw();
        }

        private async Task HandleSayHello()
        {
            try
            {
                reply = await BridgeApi.SayHelloAsync(new HelloRequest() { Name = "Иван" });
            }
            catch (Exception e)
            {
                await GlobalMsgHandler.Show(e.Message);
            }
            StateHasChanged();
        }

        private async Task HandleRedraw()
        {
            await pieChart.Clear();
            await pieChart.AddLabelsDatasetsAndUpdate(Labels, GetLineChartDataset());
        }

        private PieChart<double> pieChart;
        private PieChartDataset<double> GetLineChartDataset()
        {
            return new PieChartDataset<double>
            {
                Label = "# of randoms",
                Data = RandomizeData(),
                BackgroundColor = backgroundColors,
                BorderColor = borderColors,
                // Fill = true,
                // PointRadius = 3,
                // CubicInterpolationMode = "monotone",
            };
        }

        private readonly string[] Labels = ["Red", "Blue", "Yellow", "Green", "Purple", "Orange"];
        private readonly List<string> backgroundColors = [ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f)];
        private readonly List<string> borderColors = [ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f)];

        private static List<double> RandomizeData()
        {
            var r = new Random(DateTime.Now.Millisecond);

            return [
                r.Next( 3, 50 ) * r.NextDouble(),
                r.Next( 3, 50 ) * r.NextDouble(),
                r.Next( 3, 50 ) * r.NextDouble(),
                r.Next( 3, 50 ) * r.NextDouble(),
                r.Next( 3, 50 ) * r.NextDouble(),
                r.Next( 3, 50 ) * r.NextDouble()
            ];
        }
    }
}

using Microsoft.AspNetCore.Components;
using pax.BlazorChartJs;
using PresalesApp.Service;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages;

partial class TestPage
{
    private HelloReply? _Reply;

    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    private int _Counter = 0;

    #region UriQuery
    private const string _QueryFrom = "from";
    [SupplyParameterFromQuery(Name = _QueryFrom)] public string? From { get; set; }

    private const string _QueryTo = "to";
    [SupplyParameterFromQuery(Name = _QueryTo)] public string? To { get; set; }

    private const string _QueryPeriod = "period";
    [SupplyParameterFromQuery(Name = _QueryPeriod)] public string? PeriodType { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_QueryFrom] = Period.Start.ToString(Helper.UriDateTimeFormat),
        [_QueryTo] = Period.End.ToString(Helper.UriDateTimeFormat),
        [_QueryPeriod] = Period.Type.ToString(),
    };
    #endregion

    public Period Period = new(DateTime.Now, Enums.PeriodType.Arbitrary);

    private void _CleanStorage()
    {
        Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{_QueryFrom}");
        Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{_QueryTo}");
        Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{_QueryPeriod}");
    }

    private void _OnPeriodChanged(Period period)
    {
        Period = period;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_QueryFrom}", Period.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_QueryTo}", Period.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_QueryPeriod}", Period.Type);

        _Counter++;

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
    }

    protected override void OnInitialized()
    {
        Helper.SetFromQueryOrStorage(value: From, query: _QueryFrom, uri: Navigation.Uri, storage: Storage, param: ref Period.Start);
        Helper.SetFromQueryOrStorage(value: To, query: _QueryTo, uri: Navigation.Uri, storage: Storage, param: ref Period.End);
        Helper.SetFromQueryOrStorage(value: PeriodType, query: _QueryPeriod, uri: Navigation.Uri, storage: Storage, param: ref Period.Type);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        _HandleRedraw();
        base.OnInitialized();
    }

    private async Task _HandleSayHello()
    {
        try
        {
            _Reply = await BridgeApi.SayHelloAsync(new HelloRequest() { Name = "Иван" });
        }
        catch(Exception e)
        {
            GlobalMsgHandler.Show(e.Message);
        }

        StateHasChanged();
    }

    private int _NumberOfPoints = ChartHelpers.Colors.Count;

    private void _HandleRedraw()
    {
        _LineChartConfig.Update(labels: _GetLabels((ushort)_NumberOfPoints),
            ChartHelpers.GetLineDataset(ChartHelpers.GetRandomizedData(1), "min point", "Blue"),
            ChartHelpers.GetLineDataset(ChartHelpers.GetRandomizedData((ushort)_NumberOfPoints), "data", "Red"),
            ChartHelpers.GetLineDataset(ChartHelpers.GenerateLine((ushort)_NumberOfPoints, 40), "small line", "Yellow"),
            ChartHelpers.GetLineDataset(ChartHelpers.GenerateLine((ushort)_NumberOfPoints, 45), "big line", "Purple"));

        _PieChartConfig.Update(_GetLabels((ushort)_NumberOfPoints), ChartHelpers.GetPieDataset(ChartHelpers.GetRandomizedData((ushort)_NumberOfPoints)));
    }

    private readonly ChartJsConfig _LineChartConfig = ChartHelpers.GenerateChartConfig(ChartType.line);
    private readonly ChartJsConfig _PieChartConfig = ChartHelpers.GenerateChartConfig(ChartType.pie);

    private static string[] _GetLabels(ushort count)
    {
        var res = new string[count];

        for(var i = 0; i < count; i++)
            res[i] = ChartHelpers.Colors.ElementAt(i % ChartHelpers.Colors.Count).Key;

        return res;
    }
}

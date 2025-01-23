using Microsoft.AspNetCore.Components;
using pax.BlazorChartJs;
using PresalesApp.CustomTypes;
using PresalesApp.Service;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;

namespace PresalesApp.Web.Client.Pages;

partial class TestPage
{
    private HelloReply? _Reply;

    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    private int _Counter = 0;

    #region UriQuery

    private const string _Q_Start = "from";
    [SupplyParameterFromQuery(Name = _Q_Start)]
    public string? Start { get; set; }

    private const string _Q_End = "to";
    [SupplyParameterFromQuery(Name = _Q_End)]
    public string? End { get; set; }

    private const string _Q_PeriodType = "period";
    [SupplyParameterFromQuery(Name = _Q_PeriodType)]
    public string? PeriodType { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_Q_Start] = Period.Start.ToString(Helper.UriDateTimeFormat),
        [_Q_End] = Period.End.ToString(Helper.UriDateTimeFormat),
        [_Q_PeriodType] = Period.Type.ToString(),
    };

    #endregion

    public Helpers.Period Period = new(DateTime.Now, CustomTypes.PeriodType.Arbitrary);

    private void _CleanStorage()
    {
        Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_Start}");
        Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_End}");
        Storage.RemoveItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PeriodType}");
    }

    private void _OnPeriodChanged(Helpers.Period period)
    {
        Period = period;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_Start}", Period.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_End}", Period.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PeriodType}", Period.Type);

        _Counter++;

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
    }

    protected override void OnInitialized()
    {
        Helper.SetFromQueryOrStorage(value: Start, query: _Q_Start,
            uri: Navigation.Uri, storage: Storage, param: ref Period.Start);
        Helper.SetFromQueryOrStorage(value: End, query: _Q_End,
            uri: Navigation.Uri, storage: Storage, param: ref Period.End);
        Helper.SetFromQueryOrStorage(value: PeriodType, query: _Q_PeriodType,
            uri: Navigation.Uri, storage: Storage, param: ref Period.Type);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        _HandleRedraw();
        base.OnInitialized();
    }

    private string _Modules;

    private async Task _GetModules()
    {
        try
        {
            var response = await DistanceCalculatorApi.GetModulesAsync(new());
            _Modules = response.ToString();
        }
        catch (Exception e)
        {
            GlobalMsgHandler.Show(e.Message);
        }

        StateHasChanged();
    }

    private async Task _HandleSayHello()
    {
        try
        {
            _Reply = await BridgeApi.SayHelloAsync(new HelloRequest() { Name = "Иван" });
        }
        catch (Exception e)
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

        for (var i = 0; i < count; i++)
            res[i] = ChartHelpers.Colors.ElementAt(i % ChartHelpers.Colors.Count).Key;

        return res;
    }
}

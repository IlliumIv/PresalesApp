using Microsoft.AspNetCore.Components;
using pax.BlazorChartJs;
using PresalesApp.Service;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using Period = PresalesApp.Web.Client.Helpers.Period;
using Blazorise.Extensions;

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

        _LineChartConfig = _GenerateChartConfig(ChartType.line, [.. _Colors.Keys], null,
            _GetLineDataset(1, "min point", "Blue", 60),
            _GetLineDataset((ushort)_Colors.Count, "data", "Red"),
            _GetLineDataset((ushort)_Colors.Count, "small line", "Yellow", 40),
            _GetLineDataset((ushort)_Colors.Count, "big line", "Purple", 45));

        _PieChartConfig = _GenerateChartConfig(ChartType.pie, [.. _Colors.Keys], null,
            _GetPieDataset((ushort)_Colors.Count));
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

    private int _NumberOfPoints = _Colors.Count;

    private void _HandleRedraw()
    {
        _LineChartConfig.SetLabels(_GetLabels((ushort)_NumberOfPoints));
        _LineChartConfig.RemoveDatasets(_LineChartConfig.Data.Datasets);
        _LineChartConfig.AddDatasets(new List<ChartJsDataset>()
        {
            _GetLineDataset(1, "min point", "Blue", 60),
            _GetLineDataset((ushort)_NumberOfPoints, "data", "Red"),
            _GetLineDataset((ushort)_NumberOfPoints, "small line", "Yellow", 40),
            _GetLineDataset((ushort)_NumberOfPoints, "big line", "Purple", 45)
        });

        _PieChartConfig.SetLabels(_GetLabels((ushort)_NumberOfPoints));
        _PieChartConfig.RemoveDatasets(_PieChartConfig.Data.Datasets);
        _PieChartConfig.AddDataset(_GetPieDataset((ushort)_NumberOfPoints));
    }

    private ChartJsConfig _LineChartConfig = null!;
    private ChartJsConfig _PieChartConfig = null!;

    private static ChartJsConfig _GenerateChartConfig(ChartType chartType, string[] labels,
        ChartJsOptions? options = null, params ChartJsDataset[] datasets) => new()
        {
            Type = chartType,
            // Options = options ?? new() { Plugins = new() { Legend = new() { Display = false } } },
            Data = new()
            {
                Labels = labels,
                Datasets = datasets.ToList(),
            }
        };

    private static readonly float _BackgroundAlfa = 0.2f;
    private static readonly float _BorderAlfa = 1f;

    private static LineDataset _GetLineDataset(ushort count, string label, string color, int? value = null) => new()
    {
        Label = label,
        Data = value is null ? _GetRandomizedData(count) : _GenerateLine(count, (int)value),
        BackgroundColor = _GetColor(color, _BackgroundAlfa),
        BorderColor = _GetColor(color, _BorderAlfa),
        Fill = true,
        PointRadius = 0
    };

    private static PieDataset _GetPieDataset(ushort count) => new()
    {
        Data = _GetRandomizedData(count),
        BackgroundColor = _GetColors(count, _BackgroundAlfa),
        BorderColor = _GetColors(count, _BorderAlfa),
        Cutout = "30%"
    };

    private readonly static Dictionary<string, (byte R, byte G, byte B)> _Colors = new()
    {
        { "Red", (255, 99, 132) },
        { "Blue", (54, 162, 235) },
        { "Yellow", (255, 206, 86) },
        { "Green", (75, 192, 192) },
        { "Purple", (153, 102, 255) },
        { "Orange", (255, 159, 64) }
    };

    private static List<object> _GetRandomizedData(ushort count)
    {
        var rand = new Random();
        var res = new double[count];

        for (var i = 0; i < count; i++)
            res[i] = rand.Next(3, 50) * rand.NextDouble();

        return res.Select(d => (object)d).ToList();
    }

    private static string[] _GetLabels(ushort count)
    {
        var res = new string[count];

        for(var i = 0; i < count; i++)
            res[i] = _Colors.ElementAt(i % _Colors.Count).Key;

        return res;
    }

    private static List<object> _GenerateLine(ushort count, int value)
    {
        var res = new int[count];

        for(var i = 0; i < count; i++)
            res[i] = value;

        return res.Select(d => (object)d).ToList();
    }

    private static string _GetColor(string color, float alfa)
    {
        var (R, G, B) = _Colors[color];
        return $"rgba({R}, {G}, {B}, {alfa.ToCultureInvariantString()})";
    }

    private static List<string> _GetColors(ushort count, float alfa)
    {
        var res = new List<string>();

        for(var i = 0; i < count; i++)
        {
            var (R, G, B) = _Colors.ElementAt(i % _Colors.Count).Value;
            res.Add($"rgba({R}, {G}, {B}, {alfa.ToCultureInvariantString()})");
        }

        return res;
    }
}

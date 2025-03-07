using pax.BlazorChartJs;
using System.Globalization;

namespace PresalesApp.Web.Client.Extensions;

public static class ChartHelpers
{
    private const float _BackgroundAlfa = 0.2f;
    private const float _BorderAlfa = 1f;

    public static ChartJsConfig GenerateChartConfig(
        ChartType chartType, IList<string>? labels = null,
        ChartJsOptions? options = null, params ChartJsDataset[] datasets)
            => new()
            {
                Type = chartType,
                Options = options ?? new() { Plugins = new() { Legend = new() { Display = false } } },
                Data = new()
                {
                    Labels = labels ?? _GenerateEmptyStringList(1),
                    Datasets = [.. datasets],
                }
            };

    public static LineDataset GetLineDataset
        (IList<object> data, string label, string? color = null,
        float? backgroundAlfa = null, float? borderAlfa = null,
        bool fill = true, double pointRadius = 1)
            => new()
            {
                Label = label,
                Data = data,
                BackgroundColor = color is null
                              ? null
                              : GetColor(color, backgroundAlfa ?? _BackgroundAlfa),
                BorderColor = color is null
                          ? null
                          : GetColor(color, borderAlfa ?? _BorderAlfa),
                Fill = fill,
                PointRadius = pointRadius
            };

    public static PieDataset GetPieDataset<T>
        (IList<T> data, (byte R, byte G, byte B)? lastColor = null,
        float? backgroundAlfa = null, float? borderAlfa = null, StringOrDoubleValue? cutout = null)
            => new()
            {
                Data = (List<object>)Convert.ChangeType(data, typeof(object)),
                BackgroundColor = GetColors((ushort)data.Count, backgroundAlfa ?? _BackgroundAlfa,
                                        lastColor),
                BorderColor = GetColors((ushort)data.Count, borderAlfa ?? _BorderAlfa, lastColor),
                Cutout = cutout
            };

    public readonly static Dictionary<string, (byte R, byte G, byte B)> Colors = new()
    {
        { "DeepBlue", (5, 47, 91) },
        { "DeepPurple", (165, 14, 130) },
        { "DeepOrange", (232, 125, 55) },
        { "DarkLime", (106, 158, 31) },
        { "DarkTurquoise", (20, 150, 124) },
        { "DarkPurple", (99, 8, 78) },
        { "DarkRed", (198, 35, 36) },
        { "BlackBlue", (3, 28, 58) },
        { "DeepYellow", (255, 206, 86) },
        { "DeepGreen", (12, 90, 74) },
        { "Red", (255, 99, 132) },
        { "Blue", (54, 162, 235) },
        { "Yellow", (255, 206, 86) },
        { "Green", (75, 192, 192) },
        { "Purple", (153, 102, 255) },
        { "Orange", (255, 159, 64) }
    };

    public static List<string> GetColors
        (ushort count, float alfa, (byte R, byte G, byte B)? last = null)
    {
        var res = new List<string>();

        for (var i = 0; i < count; i++)
        {
            var (R, G, B) = Colors.ElementAt(i % Colors.Count).Value;
            res.Add($"rgba({R}, {G}, {B}, {alfa.ToStringInvariant()})");
        }

        if (res.Count != 0 && last is not null)
        {
            res.Remove(res.Last());
            res.Add($"rgba({last.Value.R}, {last.Value.G}, {last.Value.B}, " +
                $"{alfa.ToStringInvariant()})");
        }

        return res;
    }

    public static string GetColor(string color, float alfa)
        => Colors.ContainsKey(color) switch
        {
            true => $"rgba({Colors[color].R}, {Colors[color].G}, {Colors[color].B}, " +
                    $"{alfa.ToStringInvariant()})",
            false => color
        };

    public static string GetColor((byte R, byte G, byte B) color, float alfa)
        => $"rgba({color.R}, {color.G}, {color.B}, {alfa.ToStringInvariant()})";

    public static string GetColorAt(int index, float alfa)
    {
        var (R, G, B) = Colors.ElementAt(index % Colors.Count).Value;
        return $"rgba({R}, {G}, {B}, {alfa.ToStringInvariant()})";
    }

    public static string ToStringInvariant<T>(this T obj, string format = null)
        => format == null
            ? FormattableString.Invariant($"{obj}")
            : string.Format(CultureInfo.InvariantCulture.NumberFormat, $"{{0:{format}}}", obj);

    public static List<object> GenerateLine<T>(ushort count, T value) where T : notnull
    {
        var res = new T[count];

        for (var i = 0; i < count; i++)
            res[i] = value;

        return res.Select(d => (object)d).ToList();
    }

    public static void Update(this ChartJsConfig config, IList<string>? labels = null,
        params ChartJsDataset[] datasets)
    {
        config.SetLabels(labels ?? _GenerateEmptyStringList(
            datasets.MaxBy(ds => ds.Data.Count)?.Data.Count ?? 0));
        config.RemoveDatasets(config.Data.Datasets);
        config.AddDatasets(datasets);
    }

    private static List<string> _GenerateEmptyStringList(int count)
    {
        var res = new List<string>();

        for (var i = 0; i < count; i++)
            res.Add(string.Empty);

        return res;
    }

    public static List<object> GetRandomizedData(ushort count)
    {
        var rand = new Random();
        var res = new List<object>();

        for (var i = 0; i < count; i++)
            res.Add((decimal)(rand.Next(3, 50) * rand.NextDouble()));

        return res;
    }
}

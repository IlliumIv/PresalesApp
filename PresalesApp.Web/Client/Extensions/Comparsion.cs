namespace PresalesApp.Web.Client.Extensions;

public static class Comparsion
{
    public static bool GreaterThan(this decimal a, decimal b) => a > b;

    public static bool GreaterThan(this DateTime a, DateTime b) => a.TrimMilliseconds() > b.TrimMilliseconds();

    public static bool GreaterThanOrEqual(this decimal a, decimal b) => a >= b;

    public static bool GreaterThanOrEqual(this DateTime a, DateTime b) => a.TrimMilliseconds() >= b.TrimMilliseconds();

    public static bool LessThan(this decimal a, decimal b) => a < b;

    public static bool LessThan(this DateTime a, DateTime b) => a.TrimMilliseconds() < b.TrimMilliseconds();

    public static bool LessThanOrEqual(this decimal a, decimal b) => a <= b;

    public static bool LessThanOrEqual(this DateTime a, DateTime b) => a.TrimMilliseconds() <= b.TrimMilliseconds();

    public static bool Equal(this decimal a, decimal b) => a == b;

    public static bool Equal(this DateTime a, DateTime b) => a.TrimMilliseconds() == b.TrimMilliseconds();

    public static bool NotEqual(this decimal a, decimal b) => a != b;

    public static bool NotEqual(this DateTime a, DateTime b) => a.TrimMilliseconds() != b.TrimMilliseconds();

    public static bool Comparator(decimal a, decimal b, Func<decimal, decimal, bool> comparer)
        => comparer(a, b);

    public static bool Comparator(DateTime a, DateTime b, Func<DateTime, DateTime, bool> comparer)
        => comparer(a, b);

    public static DateTime TrimMilliseconds(this DateTime dt)
        => new(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);

    // TODO: Between comporator.
    /// <summary>
    /// Check if <paramref name="c"/> is between <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    private static bool Between(this decimal c, decimal a, decimal b) => (a > c && c > b) || (a < c && c < b);

    /// <summary>
    /// Check if <paramref name="c"/> is between <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    private static bool Between(this DateTime c, DateTime a, DateTime b) => (a > c && c > b) || (a < c && c < b);

    private static bool Comparator(decimal a, decimal b, decimal c, Func<decimal, decimal, decimal, bool> comparer)
        => comparer(a, b, c);

    private static bool Comparator(DateTime a, DateTime b, DateTime c, Func<DateTime, DateTime, DateTime, bool> comparer)
        => comparer(a, b, c);

    public enum ComparsionType
    {
        None = 0,
        GreaterThan = 1,
        GreaterThanOrEqual = 2,
        LessThan = 3,
        LessThanOrEqual = 4,
        Equal = 5,
        NotEqual = 6,
        Contains = 7
        // Between = 7
    }
}

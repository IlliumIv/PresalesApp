namespace PresalesApp.CustomTypes;

public partial class DecimalValue : IComparable
{
    private const decimal _NanoFactor = 1_000_000_000;

    public DecimalValue(long units, int nanos)
    {
        Units = units;
        Nanos = nanos;
    }

    public static implicit operator decimal(DecimalValue? decimalValue) => ToDecimal(decimalValue);

    public static implicit operator DecimalValue(decimal value) => FromDecimal(value);

    public static decimal ToDecimal(DecimalValue? decimalValue) => decimalValue == null
        ? 0
        : decimalValue.Units + (decimalValue.Nanos / _NanoFactor);

    public static DecimalValue FromDecimal(decimal value)
    {
        var units = decimal.ToInt64(value);
        var nanos = decimal.ToInt32((value - units) * _NanoFactor);
        return new DecimalValue(units, nanos);
    }

    public int CompareTo(object? value) => value is null
            ? 1
            : value is not DecimalValue dv
                ? throw new ArgumentException($"Argument must be {GetType().Name}")
                : decimal.Compare(this, dv);
}
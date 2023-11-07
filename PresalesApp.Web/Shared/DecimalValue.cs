namespace PresalesApp.Web.Shared
{
    public partial class DecimalValue : IComparable
    {
        private const decimal NanoFactor = 1_000_000_000;

        public DecimalValue(long units, int nanos)
        {
            Units = units;
            Nanos = nanos;
        }

        public static implicit operator decimal(DecimalValue? decimalValue) => ToDecimal(decimalValue);

        public static implicit operator DecimalValue(decimal value) => FromDecimal(value);

        public static decimal ToDecimal(DecimalValue? decimalValue)
        {
            if (decimalValue == null) return 0;
            return decimalValue.Units + decimalValue.Nanos / NanoFactor;
        }

        public static DecimalValue FromDecimal(decimal value)
        {
            var units = decimal.ToInt64(value);
            var nanos = decimal.ToInt32((value - units) * NanoFactor);
            return new DecimalValue(units, nanos);
        }

        public int CompareTo(object? value)
        {
            if (value is null)
                return 1;
            if (value is not DecimalValue dv)
                throw new ArgumentException("Argument must be DecimalValue");

            return decimal.Compare(this, dv);
        }
    }
}
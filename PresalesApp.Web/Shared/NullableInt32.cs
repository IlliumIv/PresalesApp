namespace PresalesApp.Web.Shared;

public partial class NullableInt32
{
    public static implicit operator int(NullableInt32? value) => ToInt(value);

    public static implicit operator NullableInt32(int? value) => FromInt(value);

    public static int ToInt(NullableInt32? value) => value == null ? 0 : value.Value;

    public static NullableInt32 FromInt(int? value) =>
        value is null ? new() { Null = new() } : new() { Value = (int)value };
}

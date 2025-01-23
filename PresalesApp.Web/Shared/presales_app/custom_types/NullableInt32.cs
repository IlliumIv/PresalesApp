namespace PresalesApp.CustomTypes;

public partial class NullableInt32
{
    public static implicit operator int(NullableInt32? ni) => ToInt(ni);

    public static implicit operator NullableInt32(int? i) => FromInt(i);

    public static int ToInt(NullableInt32? ni) => ni == null
        ? 0
        : ni.Value;

    public static NullableInt32 FromInt(int? i) => i is null
        ? new() { Null = new() }
        : new() { Value = (int)i };
}

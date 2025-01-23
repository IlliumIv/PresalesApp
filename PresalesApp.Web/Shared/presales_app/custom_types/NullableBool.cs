namespace PresalesApp.CustomTypes;

public partial class NullableBool
{
    public static implicit operator bool(NullableBool? nb) => ToBool(nb);

    public static implicit operator NullableBool(bool? b) => FromBool(b);

    public static bool ToBool(NullableBool? nb) => nb != null && nb.Value;

    public static NullableBool FromBool(bool? b) => b is null
        ? new() { Null = new() }
        : new() { Value = (bool)b };
}

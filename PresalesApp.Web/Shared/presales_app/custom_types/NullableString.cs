namespace PresalesApp.CustomTypes;

public partial class NullableString
{
    public static implicit operator string(NullableString? ns) => ToString(ns);

    public static implicit operator NullableString(string? s) => FromString(s);

    public static string ToString(NullableString? ns) => $"{ns?.Value}";

    public static NullableString FromString(string? s) => s is null
        ? new() { Null = new() }
        : new() { Value = s };
}

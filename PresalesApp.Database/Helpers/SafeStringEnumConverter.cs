using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace PresalesApp.Database.Helpers;

public class SafeStringEnumConverter(object defaultValue) : StringEnumConverter
{
    public object DefaultValue { get; } = defaultValue;

    public override object? ReadJson(JsonReader reader, Type objectType,
        object? existingValue, JsonSerializer serializer)
    {
        var token = JToken.ReadFrom(reader);
        var value = token.Value<string>();
        if (int.TryParse(value, out _)) return DefaultValue;
        try { return base.ReadJson(reader, objectType, existingValue, serializer); }
        catch { return DefaultValue; }
    }
}

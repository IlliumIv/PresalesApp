using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace PresalesStatistic.Helpers
{
    public class SafeStringEnumConverter : StringEnumConverter
    {
        public object DefaultValue { get; }
        public SafeStringEnumConverter(object defaultValue) => DefaultValue = defaultValue;

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
}

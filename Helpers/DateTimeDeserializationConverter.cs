using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PresalesStatistic.Helpers
{
    public class DateTimeDeserializationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            if (token.Type == JTokenType.Date)
            {
                var value = token.Value<DateTime>();
                if (value == DateTime.MinValue) return null;
                if (value.TimeOfDay == DateTime.MinValue.TimeOfDay) return value;
                return value.Add(-TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));
            }
            return existingValue;
        }
    }
}

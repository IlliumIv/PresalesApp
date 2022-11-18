using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Entities;

namespace Entities.Helpers
{
    public class CreateByStringConverter : JsonConverter
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Presale);
        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Console.WriteLine($"{objectType}");
            var token = JToken.ReadFrom(reader);
            if (token.Type == JTokenType.String)
            {
                var value = token.Value<string>();
                if (value == null || value.Length == 0) return null;
                if (objectType == typeof(Presale)) return new Presale(value.Split(";").First());
                if (objectType == typeof(Project)) return new Project(value);
            }
            return null;
        }
    }
}

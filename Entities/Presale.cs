using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic.Entities
{
    [JsonConverter(typeof(PresaleJsonConverter))]
    public class Presale
    {
        public int PresaleId { get; set; }
        public string Name;
        public Presale(string name) => Name = name;
    }

    public class PresaleJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Presale);
        public override bool CanWrite => false;
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            if (token.Type == JTokenType.String)
            {
                var value = token.Value<string>();
                if (value == null || value.Length == 0) return null;
                var presale = new Presale(value);
                return presale;
            }
            return null;
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
}

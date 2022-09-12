using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresalesStatistic.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic.Entities
{
    public class Project
    {
        public int ProjectId { get; set; }

        [JsonProperty("Код")]
        public string Number;

        [JsonProperty("Наименование")]
        public string? Name;

        [JsonProperty("Потенциал")]
        public int PotentialAmount;

        [JsonProperty("Статус")]
        public ProjectStatus Status;

        [JsonProperty("ПричинаПроигрыша")]
        public string? LossReason;

        [JsonProperty("ДатаСогласованияРТС")]
        public DateTime ApprovalByTechDirector;

        [JsonProperty("ДатаСогласованияРОП")]
        public DateTime ApprovalBySalesDirector;

        [JsonProperty("Пресейл")]
        public Presale? Presale;

        [JsonProperty("ДатаНачалаРаботыПресейла")]
        public DateTime PresaleStart;

        [JsonProperty("ОсновнойПроект")]
        [JsonConverter(typeof(MainProjectJsonConverter))]
        public Project? MainProject;

        [JsonProperty("ДействияПресейла")]
        public PresaleAction[]? Actions;

        public Project(string projectNumber) => Number = projectNumber;
    }
    public class MainProjectJsonConverter : JsonConverter
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
                var project = new Project(value);
                return project;
            }
            return null;
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) { }
    }
}

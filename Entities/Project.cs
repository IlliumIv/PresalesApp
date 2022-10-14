using Newtonsoft.Json;
using Entities.Enums;
using Entities.Helpers;

namespace Entities
{
    public class Project
    {
        public int ProjectId { get; set; }
        [JsonProperty("Код")]
        public string Number { get; private set; }
        [JsonProperty("Наименование")]
        public string? Name { get; set; }
        [JsonProperty("Потенциал")]
        public decimal PotentialAmount { get; set; }
        [JsonProperty("Статус")]
        public ProjectStatus Status { get; set; } = ProjectStatus.Unknown;
        public DateTime? LastStatusChanged { get; set; }
        [JsonProperty("ПричинаПроигрыша")]
        public string LossReason { get; set; } = string.Empty;
        [JsonProperty("ДатаСогласованияРТС")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? ApprovalByTechDirector { get; set; }
        [JsonProperty("ДатаСогласованияРОП")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? ApprovalBySalesDirector { get; set; }
        [JsonProperty("ДатаНачалаРаботыПресейла")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? PresaleStart { get; set; }
        [JsonProperty("ДействияПресейла")]
        public virtual List<PresaleAction>? Actions { get; set; }
        public int? PresaleId { get; set; }
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        [JsonProperty("ОсновнойПроект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? MainProject { get; set; }
        [JsonIgnoreSerialization]
        public virtual List<Invoice>? Invoices { get; set; }

        public Project(string number) => Number = number;
    }
}

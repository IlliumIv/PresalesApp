using Newtonsoft.Json;
using Entities.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        [JsonProperty("Номер")]
        public string Number { get; set; }
        [JsonProperty("Дата")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Date { get; set; }
        [JsonProperty("Контрагент")]
        public string Counterpart { get; set; } = string.Empty;
        [JsonProperty("СуммаРуб")]
        public decimal Amount { get; set; }
        [JsonProperty("Прибыль")]
        public decimal Profit { get; set; }
        [JsonProperty("ПрибыльИзменение")]
        [NotMapped]
        public decimal ProfitDelta { get; set; }
        public List<ProfitDelta> ProfitDeltas { get; set; } = new();
        [JsonProperty("ДатаПоследнейОплаты")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime LastPayAt { get; set; }
        [JsonProperty("ДатаПоследнейОтгрузки")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime LastShipmentAt { get; set; }
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        [JsonProperty("Проект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? Project { get; set; }
        public int? PresaleId { get; set; }
        public int? ProjectId { get; set; }
        public Invoice(string number)
        {
            Number = number;
            if (ProfitDelta != 0) ProfitDeltas.Add(new(ProfitDelta));
        }
    }
}

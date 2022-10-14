using Newtonsoft.Json;
using Entities.Helpers;

namespace Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        [JsonProperty("Номер")]
        public string Number { get; set; }
        [JsonProperty("Дата")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime Data { get; set; }
        [JsonProperty("Контрагент")]
        public string? Counterpart { get; set; }
        [JsonProperty("СуммаРуб")]
        public decimal Amount { get; set; }
        [JsonProperty("Прибыль")]
        public decimal Profit { get; set; }
        [JsonProperty("ДатаПоследнейОплаты")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? LastPay { get; set; }
        [JsonProperty("ДатаПоследнейОтгрузки")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime? LastShipment { get; set; }
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        [JsonProperty("Проект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? Project { get; set; }
        public int? PresaleId { get; set; }
        public int? ProjectId { get; set; }

        public Invoice(string number) => Number = number;
    }
}

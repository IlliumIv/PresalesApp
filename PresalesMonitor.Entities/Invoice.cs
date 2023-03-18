using Newtonsoft.Json;
using PresalesMonitor.Entities.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresalesMonitor.Entities
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
        [JsonProperty("Проект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? Project { get; set; }
        [JsonProperty("СуммаРуб")]
        public decimal Amount { get; set; }
        [JsonProperty("ДатаПоследнейОплаты")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime LastPayAt { get; set; }
        [JsonProperty("ДатаПоследнейОтгрузки")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime LastShipmentAt { get; set; }
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        [JsonProperty("ПрибыльПериоды")]
        public virtual List<ProfitPeriod>? ProfitPeriods { get; set; }
        public int? PresaleId { get; set; }
        public int? ProjectId { get; set; }
        public Invoice(string number) => Number = number;
        public decimal GetProfit() => ProfitPeriods?.Sum(p => p.Amount) ?? 0;
        public decimal GetProfit(DateTime from, DateTime to)
        {
            var firstDay = new DateTime(from.Year, from.Month, 1);
            var lastDay = to == DateTime.MaxValue ? to : new DateTime(to.Year, to.Month, 1).AddMonths(1).AddDays(-1);

            return ProfitPeriods?
                .Where(pd => pd.StartTime >= firstDay && pd.StartTime <= lastDay)?
                .Sum(pd => pd.Amount) ?? 0;
        }
    }
}

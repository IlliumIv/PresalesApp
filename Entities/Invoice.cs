using Newtonsoft.Json;
using PresalesStatistic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic.Entities
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
        public double Amount { get; set; }
        [JsonProperty("Прибыль")]
        public double Profit { get; set; }
        [JsonProperty("Пресейл")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Presale? Presale { get; set; }
        [JsonProperty("Проект")]
        [JsonConverter(typeof(CreateByStringConverter))]
        public virtual Project? Project { get; set; }
        public int? PresaleId { get; set; }
        public int? ProjectId { get; set; }

        public Invoice(string number) => Number = number;

        public void Update(Invoice invoice)
        {
            // Console.WriteLine($"Calling update to invoice {Number}");
            Amount = invoice.Amount;
            Profit = invoice.Profit;
            Presale = invoice.Presale;
            Project = invoice.Project;
        }
    }
}

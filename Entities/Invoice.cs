using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresalesStatistic.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [JsonProperty("Номер")]
        public string Number;

        [JsonProperty("Дата")]
        public DateTime Data;

        [JsonProperty("Контрагент")]
        public string? Counterpart;

        [JsonProperty("Проект")]
        [JsonConverter(typeof(MainProjectJsonConverter))]
        public Project? Project;

        [JsonProperty("СуммаРуб")]
        public int Amount;

        [JsonProperty("Прибыль")]
        public int Profit;

        [JsonProperty("Пресейл")]
        public Presale? Presale;

        public Invoice(string invoiceNumber) => Number = invoiceNumber;
    }
}

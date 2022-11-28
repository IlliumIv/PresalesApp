using PresalesMonitor.Entities.Helpers;
using Newtonsoft.Json;

namespace PresalesMonitor.Entities
{
    public class ProfitPeriod
    {
        public int ProfitPeriodId { get; set; }
        [JsonProperty("Период")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime StartTime { get; set; }
        [JsonProperty("Сумма")]
        public decimal Amount { get; set; }
        public int? InvoiceId { get; set; }
        public bool Equals(ProfitPeriod? period)
        {
            if (period is null) return false;
            if (StartTime != period.StartTime
                || Amount != period.Amount) return false;
            return true;
        }
    }
}

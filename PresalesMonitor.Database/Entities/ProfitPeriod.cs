using Newtonsoft.Json;
using PresalesMonitor.Database.Helpers;

namespace PresalesMonitor.Database.Entities
{
    public class ProfitPeriod : Entity
    {
        [JsonProperty("Период")]
        [JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime StartTime { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("Сумма")]
        public decimal Amount { get; set; } = decimal.Zero;

        public virtual Invoice? Invoice { get; set; }

        internal ProfitPeriod() { }

        public ProfitPeriod(Invoice invoice) => this.Invoice = invoice;

        public override string ToString() => $"{{\"{this.StartTime}\":\"{this.Amount}\"}}";

        public override void Save()
        {
            throw new NotImplementedException();
        }

        internal override bool TryUpdate(Controller.ReadWriteContext dbContext)
        {
            throw new NotImplementedException();
        }

        internal override ProfitPeriod Add(Controller.ReadWriteContext dbContext)
        {
            throw new NotImplementedException();
        }
    }
}

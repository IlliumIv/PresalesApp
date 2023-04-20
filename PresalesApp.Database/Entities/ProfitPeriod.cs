using Newtonsoft.Json;
using PresalesApp.Database.Helpers;

namespace PresalesApp.Database.Entities
{
    public class ProfitPeriod : Entity
    {
        [JsonProperty("Период"), JsonConverter(typeof(DateTimeDeserializationConverter))]
        public DateTime StartTime { get; set; } = new(0, DateTimeKind.Utc);

        [JsonProperty("Сумма")]
        public decimal Amount { get; set; } = decimal.Zero;

        public virtual Invoice? Invoice { get; set; }

        internal ProfitPeriod() { }

        public ProfitPeriod(Invoice invoice) => this.Invoice = invoice;

        public override string ToString() => $"{{\"{this.StartTime}\":\"{this.Amount}\"}}";

        internal override bool TryUpdateIfExist(DbController.ReadWriteContext dbContext)
        {
            throw new NotImplementedException();
        }

        internal override ProfitPeriod GetOrAddIfNotExist(DbController.ReadWriteContext dbContext)
        {
            throw new NotImplementedException();
        }
    }
}

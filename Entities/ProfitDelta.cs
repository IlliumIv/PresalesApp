namespace Entities
{
    public class ProfitDelta
    {
        public int ProfitDeltaId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public decimal Value { get; set; }
        public int InvoiceId { get; set; }
        public ProfitDelta(decimal value) => Value = value;
    }
}

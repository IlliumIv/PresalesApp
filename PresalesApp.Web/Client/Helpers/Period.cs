using Google.Protobuf.WellKnownTypes;

namespace PresalesApp.Web.Client.Helpers
{
    public class Period
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public Web.Shared.Period Translate() => new()
        {
            From = Timestamp.FromDateTime(From.ToDateTime(new TimeOnly(0, 0, 0)).ToUniversalTime()),
            To = Timestamp.FromDateTime(To.ToDateTime(new TimeOnly(23, 59, 59)).ToUniversalTime())
        };
    }
}

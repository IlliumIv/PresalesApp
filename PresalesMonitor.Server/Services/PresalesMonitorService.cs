using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using PresalesMonitor;
using PresalesMonitor.Shared;

namespace PresalesMonitor.Server.Services
{
    public class PresalesMonitorService : Presales.PresalesBase
    {
        public override Task<PresaleReply> GetStats(Presale request, ServerCallContext context)
        {
            var reply = new PresaleReply();
            var rng = new Random();
            var names = PresalesMonitor.Program.GetPresalesNames();

            reply.Presales.Add(names.Select(index => new Presale
            {
                Name = index
            }));

            return Task.FromResult(reply);
        }
    }
}

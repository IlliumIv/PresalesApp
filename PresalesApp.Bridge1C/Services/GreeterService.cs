using Grpc.Core;
using PresalesApp.Bridge1C;

namespace PresalesApp.Bridge1C.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        /*
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }
        //*/

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine("SayHello called.");
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
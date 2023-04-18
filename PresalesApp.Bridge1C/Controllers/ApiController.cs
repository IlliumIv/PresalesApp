using Grpc.Core;

namespace PresalesApp.Bridge1C.Controllers
{
    public class ApiController : PresalesAppBridge1CApi.PresalesAppBridge1CApiBase
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
using Grpc.Core;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Helpers;

namespace PresalesApp.Bridge1C.Controllers
{
    public class ApiController : Api.ApiBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine("SayHello called.");
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task<IsActionSuccess> SetProjectFunnelStage(NewProjectFunnelStage request, ServerCallContext context)
        {
            var newStage = request.NewStage;
            var projectNumber = request.ProjectNumber;

            var (IsSuccess, ErrorMessage) = await Project.SetFunnelStageAsync(newStage.Translate(), projectNumber);

            return new IsActionSuccess
            {
                IsSuccess = IsSuccess,
                ErrorMessage = ErrorMessage
            };
        }
    }
}
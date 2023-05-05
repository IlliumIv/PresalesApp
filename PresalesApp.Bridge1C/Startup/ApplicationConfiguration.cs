using PresalesApp.Bridge1C.Controllers;

namespace PresalesApp.Bridge1C.Startup
{
    public static class ApplicationConfiguration
    {
        public static WebApplication ConfigureApplication(this WebApplication app)
        {
            app.UseGrpcWeb();
            app.UseCors(); // HTTP 500

            // Configure the HTTP request pipeline.
            app.MapGrpcService<ApiController>().EnableGrpcWeb().RequireCors("cors_policy"); // HTTP 405

            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            return app;
        }
    }
}

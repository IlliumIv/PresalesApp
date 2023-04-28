using PresalesApp.Bridge1C.Controllers;
using PresalesApp.Database;
using Serilog;

namespace PresalesApp.Bridge1C.Startup
{
    public static class ServicesConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            var appSettings = new AppSettings(builder.Configuration);
            builder.Services.AddScoped(s => appSettings);

            BridgeController.Configure(appSettings);
            DbController.Configure(appSettings);
            DbController.Start();

            builder.Services.AddGrpc();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "cors_policy",
                    policy =>
                    {
                        policy
                        .WithOrigins("http://localhost:45080", "https://127.0.0.1")
                        // .AllowAnyMethod()
                        .AllowAnyHeader() // (Причина: заголовок «access-control-allow-origin» не разрешён согласно заголовку «Access-Control-Allow-Headers» из ответа CORS preflight).
                        ;
                    });
            });

            builder.Host.UseSerilog();

            return builder;
        }
    }
}

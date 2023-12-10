using PresalesApp.Service.Bridges;
using PresalesApp.Database;
using Serilog;

namespace PresalesApp.Service.Startup
{
    public static class ServicesConfiguration
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            var appSettings = new AppSettings(builder.Configuration);
            builder.Services.AddScoped(s => appSettings);

            builder.Services.AddGrpc();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "cors_policy",
                    policy =>
                    {
                        policy
                        .WithOrigins(appSettings.AllowedOrigins)
                        // .AllowAnyMethod()
                        // .AllowAnyOrigin()
                        .AllowAnyHeader() // (Причина: заголовок «access-control-allow-origin» не разрешён согласно заголовку «Access-Control-Allow-Headers» из ответа CORS preflight).
                        ;
                    });
            });

            builder.Host.UseSerilog();

            return builder;
        }
    }
}

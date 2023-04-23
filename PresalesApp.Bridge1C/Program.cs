using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using PresalesApp.Bridge1C.Controllers;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using System.Reflection;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Bridge1C
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region Logger configuration

            var log_template = "[{@l:u3}] [{@t:dd.MM.yyyy HH:mm:ss.fff} ThreadId={ThreadId}, ProcessId={ProcessId}]\n" +
                "      {#if @m <> ''}{@m}\n{#end}{@x}\n";
            var log_directory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ??
                Directory.GetCurrentDirectory();

            Log.Logger = new LoggerConfiguration()
                // https://github.com/serilog/serilog/wiki/Enrichment
                .Enrich.FromLogContext()
                .Enrich.WithThreadId() // Serilog.Enrichers.Thread
                .Enrich.WithProcessId() // Serilog.Enrichers.Process
                                        // .Enrich.WithMachineName() // Serilog.Enrichers.Environment
                .Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumStringLength(100)
                .Destructure.ToMaximumCollectionCount(10)
                .MinimumLevel.Verbose()
                .WriteTo.Async(a => a.Console(
                    formatter: new ExpressionTemplate(log_template, theme: TemplateTheme.Code),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(log_template),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    path: $"{log_directory}/Logs/Parser.log"))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(log_template),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    path: $"{log_directory}/Logs/Error.log"))
                .CreateLogger()
                // .ForContext<Program>()
                ;
            #endregion
            #region Json Serializer configuration

            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
            };
            #endregion

            DbController.Start(Log.Logger);
            BridgeController.Start<Project>(TimeSpan.Zero);
            BridgeController.Start<CacheLog>(TimeSpan.FromSeconds(5));
            BridgeController.Start<Invoice>(TimeSpan.FromSeconds(10));

            #region Web App configuration and run
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "cors_policy",
                    policy =>
                    {
                        policy
                        .WithOrigins(Settings.Default.WebServerAddress)
                        // .AllowAnyMethod()
                        .AllowAnyHeader() // (Причина: заголовок «access-control-allow-origin» не разрешён согласно заголовку «Access-Control-Allow-Headers» из ответа CORS preflight).
                        ;
                    });
            });

            var app = builder.Build();

            app.UseGrpcWeb();
            app.UseCors(); // HTTP 500

            // Configure the HTTP request pipeline.
            app.MapGrpcService<ApiController>().EnableGrpcWeb()
                .RequireCors("cors_policy"); // HTTP 405

            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
            #endregion
        }
    }
}
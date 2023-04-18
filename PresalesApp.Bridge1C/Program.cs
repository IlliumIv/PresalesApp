using PresalesApp.Bridge1C.Services;
using Serilog;
using Serilog.Templates.Themes;
using Serilog.Templates;
using Newtonsoft.Json;
using PresalesMonitor.Database;
using System.Reflection;
using PresalesMonitor.Database.Entities;
using PresalesApp.Bridge1C.Workers;
using PresalesMonitor.Database.Entities.Updates;

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

            Controller.Start(Log.Logger);
            UpdatePuller.Start<Project>(TimeSpan.Zero);
            UpdatePuller.Start<CacheLog>(TimeSpan.FromSeconds(5));
            UpdatePuller.Start<Invoice>(TimeSpan.FromSeconds(10));

            #region Web App configuration and run
            var builder = WebApplication.CreateBuilder(args);

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "CORS Allow-All",
                    policy =>
                    {
                        policy
                        .WithOrigins(Settings.Default.WebServerAddress)
                        // .AllowAnyMethod()
                        .AllowAnyHeader() // (�������: ��������� �access-control-allow-origin� �� �������� �������� ��������� �Access-Control-Allow-Headers� �� ������ CORS preflight).
                        ;
                    });
            });

            var app = builder.Build();

            app.UseCors(); // HTTP 500
            app.UseGrpcWeb();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>().EnableGrpcWeb()
                .RequireCors("CORS Allow-All"); // HTTP 405

            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
            #endregion
        }
    }
}
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PresalesMonitor.Entities;
using Serilog;
using System.Drawing;
using Serilog.Templates;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates.Themes;

namespace PresalesMonitor
{
    public static class Extensions
    {
        public static void Error(this ILogger logger, Exception ex) => logger.Error(ex, "");
        public static void Fatal(this ILogger logger, Exception ex) => logger.Fatal(ex, "");
    }

    public class Synchronizer
    {
        public static readonly FileInfo _workLog = new("Parser.log");
        public static readonly FileInfo _errorLog = new("Error.log");
        // await File.AppendAllTextAsync(_errorLog.FullName, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] {e}\n\n");

        static void Main(string[] args)
        {


            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var template = "[{@t:dd.MM.yyyy HH:mm:ss.fff}] [{@l:u3}] {#if @m <> ''}{@m}\n{#end}{@x}\n";

            var logger = new LoggerConfiguration()
                // https://github.com/serilog/serilog/wiki/Enrichment
                .Enrich.FromLogContext()
                // .Enrich.WithThreadId() // Serilog.Enrichers.Thread
                // .Enrich.WithProcessId() // Serilog.Enrichers.Process
                // .Enrich.WithMachineName() // Serilog.Enrichers.Environment 
                .Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumStringLength(100)
                .Destructure.ToMaximumCollectionCount(10)
                .MinimumLevel.Verbose()
                .WriteTo.Async(a => a.Console(
                    formatter: new ExpressionTemplate(template, theme: TemplateTheme.Code),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(template),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    path: $"Logs/{_workLog}"))
                .WriteTo.Async(a => a.File(
                    formatter: new ExpressionTemplate(template),
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    path: $"Logs/{_errorLog}"))
                // .ReadFrom.Configuration(configuration)
                .CreateLogger().ForContext<Synchronizer>();

            try
            {
                Exception e = new();
                e = null;
                // Console.WriteLine(e.StackTrace);
            }
            catch (Exception ex)
            {
                logger.Verbose("Verbose");
                logger.Debug("LogDebug");
                logger.Information("LogInformation");
                logger.Warning("LogWarning");
                logger.Error(ex);
                logger.Error(ex, "LogError");
                logger.Fatal(ex);
                logger.Fatal(ex, "LogFatal");
            }
            //*/

            Console.ReadLine();
        }

        /*
        public static async void Start(TimeSpan delay)
        {
            await RunUpdates();
            PeriodicTimer periodicTimer = new(delay);
            while (await periodicTimer.WaitForNextTickAsync())
                await RunUpdates();
        }
        private static async Task RunUpdates()
        {
            if (Settings.TryGetSection<Settings.Application>(
                out ConfigurationSection? r) && r != null)
            {
                Settings.Application appSettings = (Settings.Application)r;



                var projUpdate = RunPeriodicalProjectsUpdate();
                var invUpdate = RunPeriodicalInvoicesUpdate();

                await Task.WhenAll(projUpdate, invUpdate);
                appSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(appSettings.SectionInformation.Name);
            }
            else throw new ConfigurationErrorsException();
        }
        private static async Task RunPeriodicalProjectsUpdate()
        {
            if (Settings.TryGetSection<Settings.Application>(
                out ConfigurationSection? r) && r != null)
            {
                Settings.Application appSettings = (Settings.Application)r;
                Settings.
                var to = (DateTime.Now - appSettings.ProjectsUpdatedAt).TotalDays > 1
                    ? appSettings.ProjectsUpdatedAt.AddDays(1)
                    : DateTime.Now;

                var isSuccessful = await Update<Project>(appSettings.ProjectsUpdatedAt, to);
                if (isSuccessful)
                    appSettings.ProjectsUpdatedAt = to;
            }
            else throw new ConfigurationErrorsException();
        }
        private static async Task RunPeriodicalInvoicesUpdate()
        {
            if (Settings.TryGetSection<Settings.Application>(
                out ConfigurationSection? r) && r != null)
            {
                Settings.Application appSettings = (Settings.Application)r;
                var to = (DateTime.Now - appSettings.InvoicesUpdatedAt).TotalDays > 1
                    ? appSettings.InvoicesUpdatedAt.AddDays(1)
                    : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour - 1, 0, 0);

                var isSuccessful = false;
                if (to.Date != appSettings.InvoicesUpdatedAt.Date || to.Hour != appSettings.InvoicesUpdatedAt.Hour && to.Minute >= 20)
                    isSuccessful = await Update<Invoice>(appSettings.InvoicesUpdatedAt, to);

                if (isSuccessful)
                    appSettings.InvoicesUpdatedAt = to;
            }
            else throw new ConfigurationErrorsException();
        }
        public static async Task<bool> Update<T>(DateTime from, DateTime to) where T : notnull
        {
            Console.WriteLine($"Get {typeof(T).Name}s from: {from}\tto: {to}");
            if (Settings.TryGetSection<Settings.Connection>(
                    out ConfigurationSection? r) && r != null)
            {
                /*
                var connSettings = (Settings.Connection)r;
                var _auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                    $"{connSettings.Username}:{connSettings.Password}"));
                var httpClient = new HttpClient { BaseAddress = new Uri($"http://{connSettings.Url}") };
                var message = new HttpRequestMessage(HttpMethod.Get,
                    $"trade/hs/API/Get{typeof(T).Name}s?" +
                    $"begin={from:yyyy-MM-ddTHH:mm:ss}" +
                    $"&end={to:yyyy-MM-ddTHH:mm:ss}");
                message.Headers.Add("Authorization", $"Basic {_auth}");
                await File.AppendAllTextAsync(_workLog.FullName, $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Request: {message.RequestUri}\n\n");
                var httpResponse = await httpClient.SendAsync(message);
                if (httpResponse.IsSuccessStatusCode)
                    //*---/
                if (true)
                {
                    // var @string = await httpResponse.Content.ReadAsStringAsync();
                    var @string = typeof(T).Name switch
                    {
                        "Project" => await OneAssMock.GetProjectsAsync(),
                        "Invoice" => await OneAssMock.GetInvoicesAsync(),
                        _ => throw new NotImplementedException()
                    };

                    var dynObjects = JsonConvert.DeserializeObject<dynamic>(
                            @string,
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc });

                    if (dynObjects != null)
                        foreach (var obj in dynObjects)
                            DbMock.Proceed(obj.ToObject<T>());

                    return true;
                }
                // else throw new Exception($"{httpResponse.StatusCode}. {httpResponse.Content}");
            }
            else throw new ConfigurationErrorsException();
        }
                //*/
    }
    internal static class OneAssMock
    {
        public static async Task<string> GetInvoicesAsync()
        {
            await Task.Delay(100);
            return File.ReadAllText("Invoices.txt");
        }
        public static async Task<string> GetProjectsAsync()
        {
            await Task.Delay(100);
            return File.ReadAllText("Projects.txt");
        }
    }
}

using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using PresalesApp.Service.Bridges;
using PresalesApp.Service.Startup;
using PresalesApp.Web.Shared.Startup;

namespace PresalesApp.Service;

public class Program
{
    public static void Main(string[] args)
    {
        SerilogConfiguration.ConfigureLogger<Program>();
        JsonConvertConfiguration.ConfigureJsonConvert();

        var builder = WebApplication.CreateBuilder(args).ConfigureServices();

        var appSettings = new AppSettings(builder.Configuration);

        new Bridge1C(appSettings).Start<Project>(TimeSpan.Zero);
        new Bridge1C(appSettings).Start<CacheLog>(TimeSpan.FromSeconds(5));
        new Bridge1C(appSettings).Start<Invoice>(TimeSpan.FromSeconds(10));

        DbController.Configure(appSettings);
        DbController.Start();

        builder.Build().ConfigureApplication().Run();
    }
}
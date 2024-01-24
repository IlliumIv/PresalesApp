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

        DbController.Configure(appSettings);
        DbController.Start();

        new Bridge1C(appSettings).Start<Project>(TimeSpan.Zero);
        new Bridge1C(appSettings).Start<InvoicesCache>(TimeSpan.FromSeconds(5));
        new Bridge1C(appSettings).Start<Invoice>(TimeSpan.FromSeconds(10));

        builder.Build().ConfigureApplication().Run();
    }
}
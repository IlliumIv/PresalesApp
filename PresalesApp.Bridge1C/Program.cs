using PresalesApp.Bridge1C.Controllers;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;
using PresalesApp.Bridge1C.Startup;
using PresalesApp.Web.Shared.Startup;

namespace PresalesApp.Bridge1C
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerilogConfiguration.ConfigureLogger<Program>();
            JsonConvertConfiguration.ConfigureJsonConvert();

            var builder = WebApplication.CreateBuilder(args).ConfigureServices();
            
            BridgeController.Start<Project>(TimeSpan.Zero);
            BridgeController.Start<CacheLog>(TimeSpan.FromSeconds(5));
            BridgeController.Start<Invoice>(TimeSpan.FromSeconds(10));

            builder.Build().ConfigureApplication().Run();
        }
    }
}
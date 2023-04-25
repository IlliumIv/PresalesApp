using PresalesApp.Bridge1C.Controllers;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;
using PresalesApp.Bridge1C.Startup;

namespace PresalesApp.Bridge1C
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerilogConfiguration.ConfigureLogger();
            JsonConvertConfiguration.ConfigureJsonConvert();

            DbController.Start();
            BridgeController.Start<Project>(TimeSpan.Zero);
            BridgeController.Start<CacheLog>(TimeSpan.FromSeconds(5));
            BridgeController.Start<Invoice>(TimeSpan.FromSeconds(10));

            WebApplication.CreateBuilder(args).ConfigureServices().Build().ConfigureApplication().Run();
        }
    }
}
using PresalesApp.Web.Server.Startup;
using PresalesApp.Web.Shared.Startup;

namespace PresalesApp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerilogConfiguration.ConfigureLogger<Program>();
            WebApplication.CreateBuilder(args).ConfigureServices().Build().ConfigureApplication().Run();
        }
    }
}
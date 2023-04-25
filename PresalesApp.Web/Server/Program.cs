using PresalesApp.Web.Server.Startup;

namespace PresalesApp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SerilogConfiguration.ConfigureLogger();
            WebApplication.CreateBuilder(args).ConfigureServices().Build().ConfigureApplication().Run();
        }
    }
}
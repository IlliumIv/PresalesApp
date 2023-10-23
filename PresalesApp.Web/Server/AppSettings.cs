using PresalesApp.Database;

namespace PresalesApp.Web.Server
{
    public class AppSettings(IConfiguration configuration) : Settings(configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public string Issuer => _configuration.GetValue<string>("TokenParameters:Issuer") ?? string.Empty;
        public string Audience => _configuration.GetValue<string>("TokenParameters:Audience") ?? string.Empty;
        public string SecretKey => _configuration.GetValue<string>("TokenParameters:SecretKey") ?? string.Empty;
        public TimeSpan Expiry => _configuration.GetValue<TimeSpan>("TokenParameters:Expiry");
    }
}

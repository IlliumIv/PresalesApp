using PresalesApp.Database;

namespace PresalesApp.Web.Server
{
    public class AppSettings : Settings
    {
        private readonly IConfiguration _configuration;

        public AppSettings(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        public string Issuer { get  { return _configuration.GetValue<string>("TokenParameters:Issuer"); }}
        public string Audience { get { return _configuration.GetValue<string>("TokenParameters:Audience"); }}
        public string SecretKey { get { return _configuration.GetValue<string>("TokenParameters:SecretKey"); }}
        public TimeSpan Expiry { get { return _configuration.GetValue<TimeSpan>("TokenParameters:Expiry"); }}
    }
}

using PresalesApp.Database;

namespace PresalesApp.Bridge1C
{
    public class AppSettings : Settings
    {
        private readonly IConfiguration _configuration;

        public AppSettings(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }

        public TimeSpan RequestsTimeout { get { return _configuration.GetValue<TimeSpan>("Bridge1C:RequestsTimeout"); } }
        public TimeSpan ProjectsUpdateDelay { get { return _configuration.GetValue<TimeSpan>("Bridge1C:ProjectsUpdateDelay"); } }
        public string Host { get { return _configuration.GetValue<string>("Bridge1C:Host"); } }
        public string Password { get { return _configuration.GetValue<string>("Bridge1C:Password"); } }
        public string Username { get { return _configuration.GetValue<string>("Bridge1C:Username"); } }
    }
}

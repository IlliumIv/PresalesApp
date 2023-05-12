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

        public TimeSpan RequestsTimeout => _configuration.GetValue<TimeSpan>("Bridge1C:RequestsTimeout");
        public TimeSpan ProjectsUpdateDelay => _configuration.GetValue<TimeSpan>("Bridge1C:ProjectsUpdateDelay");
        public new string Host => _configuration.GetValue<string>("Bridge1C:Host") ?? string.Empty;
        public new string Password => _configuration.GetValue<string>("Bridge1C:Password") ?? string.Empty;
        public new string Username => _configuration.GetValue<string>("Bridge1C:Username") ?? string.Empty;
        public string[] AllowedOrigins => _configuration.GetSection("Bridge1C:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        public bool SynchronizationEnabled => _configuration.GetValue<bool>("Bridge1C:SynchronizationEnabled");
    }
}

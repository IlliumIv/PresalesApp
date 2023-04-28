using Microsoft.Extensions.Configuration;

namespace PresalesApp.Database
{
    public class Settings
    {
        private readonly IConfiguration _configuration;

        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Host { get { return _configuration.GetValue<string>("DbConnection:Host"); } }
        public int Port { get { return _configuration.GetValue<int>("DbConnection:Port"); } }
        public string Database { get { return _configuration.GetValue<string>("DbConnection:Database"); } }
        public string Username { get { return _configuration.GetValue<string>("DbConnection:Username"); } }
        public string Password { get { return _configuration.GetValue<string>("DbConnection:Password"); } }
    }
}

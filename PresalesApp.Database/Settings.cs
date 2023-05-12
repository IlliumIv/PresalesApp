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

        public string Host => _configuration.GetValue<string>("DbConnection:Host") ?? string.Empty;
        public int Port => _configuration.GetValue<int>("DbConnection:Port");
        public string Database => _configuration.GetValue<string>("DbConnection:Database") ?? string.Empty;
        public string Username => _configuration.GetValue<string>("DbConnection:Username") ?? string.Empty;
        public string Password => _configuration.GetValue<string>("DbConnection:Password") ?? string.Empty;
    }
}

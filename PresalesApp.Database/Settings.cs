using Microsoft.Extensions.Configuration;

namespace PresalesApp.Database
{
    public class Settings(IConfiguration configuration)
    {
        private readonly IConfiguration _Configuration = configuration;

        public string DbHost => _Configuration.GetValue<string>("DbConnection:Host") ?? string.Empty;
        public int DbPort => _Configuration.GetValue<int>("DbConnection:Port");
        public string DbName => _Configuration.GetValue<string>("DbConnection:Database") ?? string.Empty;
        public string DbUsername => _Configuration.GetValue<string>("DbConnection:Username") ?? string.Empty;
        public string DbPassword => _Configuration.GetValue<string>("DbConnection:Password") ?? string.Empty;
    }
}

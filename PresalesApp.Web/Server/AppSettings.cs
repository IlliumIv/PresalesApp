using PresalesApp.Database;

namespace PresalesApp.Web.Server;

public class AppSettings(IConfiguration configuration) : Settings(configuration)
{
    private readonly IConfiguration _Configuration = configuration;

    public string Issuer => _Configuration.GetValue<string>("TokenParameters:Issuer") ?? string.Empty;
    public string Audience => _Configuration.GetValue<string>("TokenParameters:Audience") ?? string.Empty;
    public string SecretKey => _Configuration.GetValue<string>("TokenParameters:SecretKey") ?? string.Empty;
    public TimeSpan Expiry => _Configuration.GetValue<TimeSpan>("TokenParameters:Expiry");
}

namespace PresalesApp.Web.Client.Startup;

public class Settings(IConfiguration configuration)
{
    private readonly IConfiguration _Configuration = configuration;

    public string ServiceHost => _Configuration.GetValue<string>("Service:Host") ?? string.Empty;
    public int ServicePort => _Configuration.GetValue<int>("Service:Port");
    public bool UseSSL => _Configuration.GetValue<bool>("Service:UseSSL");
}

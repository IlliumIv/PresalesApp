using PresalesApp.Database;

namespace PresalesApp.Service;

public class AppSettings(IConfiguration configuration) : Settings(configuration)
{
    private readonly IConfiguration _Configuration = configuration;
    public Macroscop Macroscop = new(configuration);
    public _1C _1C = new(configuration);

    public string[] AllowedOrigins => _Configuration.GetSection("Service:AllowedOrigins").Get<string[]>() ?? [];
}

public class Macroscop(IConfiguration configuration)
{
    private readonly IConfiguration _Configuration = configuration;

    public string Host => _Configuration.GetValue<string>("Service:Macroscop:Host") ?? string.Empty;
    public int Port => _Configuration.GetValue<int>("Service:Macroscop:Port");
    public bool UseSSL => _Configuration.GetValue<bool>("Service:Macroscop:UseSSL");
    public string Username => _Configuration.GetValue<string>("Service:Macroscop:Username") ?? string.Empty;
    public string Password => _Configuration.GetValue<string>("Service:Macroscop:Password") ?? string.Empty;
    public bool IsActiveDirectoryUser => _Configuration.GetValue<bool>("Service:Macroscop:IsActiveDirectoryUser");
    public string EntranceChannelId => _Configuration.GetValue<string>("Service:Macroscop:EntranceChannelId") ?? string.Empty;
    public string EventId => _Configuration.GetValue<string>("Service:Macroscop:EventId") ?? string.Empty;
}

public class _1C(IConfiguration configuration)
{
    private readonly IConfiguration _Configuration = configuration;

    public TimeSpan RequestsTimeout => _Configuration.GetValue<TimeSpan>("Service:1C:RequestsTimeout");
    public TimeSpan ProjectsUpdateDelay => _Configuration.GetValue<TimeSpan>("Service:1C:ProjectsUpdateDelay");
    public bool Enabled => _Configuration.GetValue<bool>("Service:1C:Enabled");
    public string Host => _Configuration.GetValue<string>("Service:1C:Host") ?? string.Empty;
    public string Username => _Configuration.GetValue<string>("Service:1C:Username") ?? string.Empty;
    public string Password => _Configuration.GetValue<string>("Service:1C:Password") ?? string.Empty;
}

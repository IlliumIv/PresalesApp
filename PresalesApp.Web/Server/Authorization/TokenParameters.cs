namespace PresalesApp.Web.Server.Authorization;

public class TokenParameters(AppSettings settings)
{
    private readonly AppSettings _Settings = settings;

    public string Issuer => _Settings.Issuer; // Издатель

    public string Audience => _Settings.Audience; // Подписчик

    public string SecretKey => _Settings.SecretKey;

    public DateTime Expiry => DateTime.Now + _Settings.Expiry;
}

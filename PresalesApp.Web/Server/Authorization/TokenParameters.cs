namespace PresalesApp.Web.Server.Authorization
{
    public class TokenParameters(AppSettings settings)
    {
        private readonly AppSettings _settings = settings;

        public string Issuer => _settings.Issuer; // Издатель
        public string Audience => _settings.Audience; // Подписчик
        public string SecretKey => _settings.SecretKey;
        public DateTime Expiry => DateTime.Now + _settings.Expiry;
    }
}

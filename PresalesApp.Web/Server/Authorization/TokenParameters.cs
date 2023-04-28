namespace PresalesApp.Web.Server.Authorization
{
    public class TokenParameters
    {
        private readonly AppSettings _settings;

        public TokenParameters(AppSettings settings)
        {
            _settings = settings;
        }
        public string Issuer => _settings.Issuer; // Издатель
        public string Audience => _settings.Audience; // Подписчик
        public string SecretKey => _settings.SecretKey;
        public DateTime Expiry => DateTime.Now + _settings.Expiry;
    }
}

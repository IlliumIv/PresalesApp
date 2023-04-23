namespace PresalesApp.Database
{
    public class TokenParameters
    {
        public string Issuer => Settings.Default.Issuer; // Издатель
        public string Audience => Settings.Default.Audience; // Подписчик
        public string SecretKey => Settings.Default.SecretKey;
        public DateTime Expiry => DateTime.Now + Settings.Default.Expiry;
    }
}

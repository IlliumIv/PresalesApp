using Newtonsoft.Json;

namespace PresalesApp.Service.Startup
{
    public static class JsonConvertConfiguration
    {
        public static void ConfigureJsonConvert()
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };
            };
        }
    }
}

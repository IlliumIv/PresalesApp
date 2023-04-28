using Newtonsoft.Json;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;
using System.Text;

namespace PresalesApp.Bridge1C.Controllers
{
    public static class BridgeController
    {
        public static AppSettings _settings { get; private set; }
        public static void Configure(AppSettings settings) => _settings = settings;
        public static void Start<T>(TimeSpan startDelay) where T : Entity
        {
            if (startDelay.TotalMilliseconds <= 0) startDelay = TimeSpan.FromMilliseconds(1);

            new Task(async () =>
            {
                var periodic_timer = new PeriodicTimer(startDelay);
                var next_update_delay = _settings.RequestsTimeout;

                while (await periodic_timer.WaitForNextTickAsync())
                {
                    periodic_timer.Dispose();
                    next_update_delay = _settings.RequestsTimeout;
                    try
                    {
                        Update update = typeof(T).Name switch
                        {
                            nameof(Project) => new ProjectsUpdate(),
                            nameof(CacheLog) => new CacheLogsUpdate(),
                            nameof(Invoice) => new CacheLog(DateTime.UtcNow),
                            _ => throw new NotImplementedException(),
                        };

                        if (TryGetData(
                            result: out List<T> items,
                            update: ref update))
                        {
                            foreach (var item in items)
                            {
                                item.Save();
                            }
                        }


                        update.Save();

                        if (DateTime.UtcNow - update.SynchronizedTo > _settings.RequestsTimeout)
                            next_update_delay = TimeSpan.FromMilliseconds(1);

                        Log.Information($"{typeof(T).Name}s has been updated at " +
                            $"{update.SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}");
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "");
                    }

                    periodic_timer = new PeriodicTimer(next_update_delay);
                }
            }).Start();

            Log.Information($"{typeof(BridgeController).FullName} for {typeof(T).Name}s started.");
        }

        private static bool TryGetData<T>(out List<T> result, ref Update update) where T : Entity
        {
            result = new List<T>();

            var (request, period) = DefineVariables<T>(ref update);

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{_settings.Username}:{_settings.Password}"));

            var http_client = new HttpClient { BaseAddress = new Uri($"http://{_settings.Host}") };

            var request_uri = $"trade/hs/API/{request}?" +
                $"begin={period.from.ToLocalTime():yyyy-MM-ddTHH:mm:ss}" +
                $"&end={period.to.ToLocalTime():yyyy-MM-ddTHH:mm:ss}";
            var message = new HttpRequestMessage(HttpMethod.Get, request_uri);
            message.Headers.Add("Authorization", $"Basic {auth}");

            Log.Debug($"{http_client.BaseAddress}{message.RequestUri}");

            http_client.Timeout = TimeSpan.FromMinutes(3);
            HttpResponseMessage response = http_client.SendAsync(message).Result;

            if (response.IsSuccessStatusCode)
            {
                var objects = JsonConvert.DeserializeObject<dynamic>(
                    response.Content.ReadAsStringAsync().Result);
                if (objects != null)
                    foreach (var obj in objects)
                        result.Add(obj.ToObject<T>());
                return true;
            }
            else throw new Exception($"Request: {message.RequestUri}\n" +
                $"\tStatusCode: {response.StatusCode}\n" +
                $"\tContent: {response.Content.ReadAsStringAsync().Result.Replace("\r", "").Replace("\n", "")}");
        }

        private static (string request, (DateTime from, DateTime to) period)
            DefineVariables<T>(ref Update update) where T : Entity
        {
            return typeof(T).Name switch
            {
                nameof(Project) => ("GetProjects", GetPeriod<ProjectsUpdate>(ref update)),
                nameof(CacheLog) => ("GetLog", GetPeriod<CacheLogsUpdate>(ref update)),
                nameof(Invoice) => ("GetInvoices", GetPeriod<CacheLog>(ref update)),
                _ => throw new NotImplementedException()
            };

        }

        private static (DateTime from, DateTime to) GetPeriod<T>(ref Update update) where T : Update
        {
            var previous_update = update.GetPrevious();

            var from = previous_update.SynchronizedTo;
            var to = (DateTime.UtcNow - previous_update.Timestamp).TotalDays > 1 ?
                previous_update.Timestamp.AddDays(1) : DateTime.UtcNow;

            update.Timestamp = to;

            if (update.GetType().Name == nameof(CacheLog))
            {
                var cache_log = previous_update as CacheLog;
                if (cache_log != null)
                {
                    to = (cache_log.PeriodEnd - cache_log.SynchronizedTo).TotalDays > 1 ?
                        cache_log.SynchronizedTo.AddDays(1) : cache_log.PeriodEnd;
                    to = to > DateTime.UtcNow ? DateTime.UtcNow : to;
                    update = cache_log;
                }
            }

            update.SynchronizedTo = to;

            // В 1С включен механизм фоновой записи, запрашиваем обновления проектов с задержкой, чтобы не промахнуться.
            var delay = typeof(T).Name == nameof(Project) ? _settings.ProjectsUpdateDelay : TimeSpan.Zero;

            return (from - delay, to - delay);
        }
    }
}
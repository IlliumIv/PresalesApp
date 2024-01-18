using Newtonsoft.Json;
using PresalesApp.Database;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;
using System.Text;

namespace PresalesApp.Service.Bridges;

public class Bridge1C(AppSettings appSettings)
{
    private readonly AppSettings _Settings = appSettings;

    public void Start<T>(TimeSpan startDelay) where T : Entity
    {
        if(_Settings is null || !_Settings._1C.Enabled)
        {
            return;
        }

        if(startDelay.TotalMilliseconds <= 0)
        {
            startDelay = TimeSpan.FromMilliseconds(1);
        }

        new Task(async () =>
        {
            var periodic_timer = new PeriodicTimer(startDelay);
            var next_update_delay = _Settings._1C.RequestsTimeout;

            while (await periodic_timer.WaitForNextTickAsync())
            {
                periodic_timer.Dispose();
                next_update_delay = _Settings._1C.RequestsTimeout;
                try
                {
                    Update update = typeof(T).Name switch
                    {
                        nameof(Project) => new ProjectsUpdate(),
                        nameof(InvoicesCache) => new CacheLogsUpdate(),
                        nameof(Invoice) => new InvoicesCache(DateTime.UtcNow),
                        _ => throw new NotImplementedException(),
                    };

                    if (_TryGetData(
                        result: out List<T> items,
                        update: ref update))
                    {
                        foreach (var item in items)
                        {
                            item.Save();
                        }
                    }

                    update.Save();

                    if(DateTime.UtcNow - update.SynchronizedTo > _Settings._1C.RequestsTimeout)
                    {
                        next_update_delay = TimeSpan.FromMilliseconds(1);
                    }

                    Log.Information($"{typeof(T).Name}s has been updated at " +
                        $"{update.SynchronizedTo.ToLocalTime():dd.MM.yyyy HH:mm:ss.fff zzz}");
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Error while processing update for {typeof(T).Name}");
                }

                periodic_timer = new PeriodicTimer(next_update_delay);
            }
        }).Start();

        Log.Information($"{typeof(Bridge1C).FullName} for {typeof(T).Name}s started.");
    }

    private bool _TryGetData<T>(out List<T> result, ref Update update) where T : Entity
    {
        result = [];

        var (request, period) = _DefineVariables<T>(ref update);

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{_Settings._1C.Username}:{_Settings._1C.Password}"));

        var http_client = new HttpClient { BaseAddress = new Uri($"http://{_Settings._1C.Host}") };

        var request_uri = $"trade/hs/API/{request}?" +
            $"begin={period.from.ToLocalTime():yyyy-MM-ddTHH:mm:ss}" +
            $"&end={period.to.ToLocalTime():yyyy-MM-ddTHH:mm:ss}";
        var message = new HttpRequestMessage(HttpMethod.Get, request_uri);
        message.Headers.Add("Authorization", $"Basic {auth}");

        Log.Debug($"{http_client.BaseAddress}{message.RequestUri}");

        http_client.Timeout = TimeSpan.FromMinutes(3);
        var response = http_client.Send(message);

        if (response.IsSuccessStatusCode)
        {
            var objects = JsonConvert.DeserializeObject<dynamic>(
                response.Content.ReadAsStringAsync().Result);
            if(objects != null)
            {
                foreach (var obj in objects)
                {
                    result.Add(obj.ToObject<T>());
                }
            }

            return true;
        }
        else
        {
            throw new Exception($"Request: {message.RequestUri}\n" +
            $"\tStatusCode: {response.StatusCode}\n" +
            $"\tContent: {response.Content.ReadAsStringAsync().Result.Replace("\r", "").Replace("\n", "")}");
        }
    }

    private (string request, (DateTime from, DateTime to) period)
        _DefineVariables<T>(ref Update update) where T : Entity
    {
        return typeof(T).Name switch
        {
            nameof(Project) => ("GetProjects", _GetPeriod<ProjectsUpdate>(ref update)),
            nameof(InvoicesCache) => ("GetLog", _GetPeriod<CacheLogsUpdate>(ref update)),
            nameof(Invoice) => ("GetInvoices", _GetPeriod<InvoicesCache>(ref update)),
            _ => throw new NotImplementedException()
        };

    }

    private (DateTime from, DateTime to) _GetPeriod<T>(ref Update update) where T : Update
    {
        var previous_update = update.GetPrevious();

        var from = previous_update.SynchronizedTo;
        var to = (DateTime.UtcNow - previous_update.Timestamp).TotalDays > 1 ?
            previous_update.Timestamp.AddDays(1) : DateTime.UtcNow;

        update.Timestamp = to;

        if (update.GetType().Name == nameof(InvoicesCache))
        {
            if (previous_update is InvoicesCache cache_log)
            {
                to = (cache_log.PeriodEnd - cache_log.SynchronizedTo).TotalDays > 1 ?
                    cache_log.SynchronizedTo.AddDays(1) : cache_log.PeriodEnd;
                to = to > DateTime.UtcNow ? DateTime.UtcNow : to;
                update = cache_log;
            }
        }

        update.SynchronizedTo = to;

        // В 1С включен механизм фоновой записи, запрашиваем обновления проектов с задержкой, чтобы не промахнуться.
        var delay = typeof(T).Name == nameof(Project) ? _Settings._1C.ProjectsUpdateDelay : TimeSpan.Zero;

        return (from - delay, to - delay);
    }
}
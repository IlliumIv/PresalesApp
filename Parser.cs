using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PresalesStatistic.Entities;
using System.Configuration;
using System.Text;

namespace PresalesStatistic
{
    public class Parser
    {
        private static DateTime _lastProjectsUpdate;
        private static DateTime _lastInvoicesUpdate;
        private static string? _auth;
        private static readonly JsonSerializerSettings deserializeSettings = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        public static async Task RunAsync(int delay = 600000)
        {
            Settings.TryGetSection<Settings.Application>(out ConfigurationSection? r);
            if (r == null) return;
            var appSettings = (Settings.Application)r;

            _lastProjectsUpdate = appSettings.PreviosUpdate;
            _lastInvoicesUpdate = appSettings.PreviosUpdate;

            while (true)
            {
                var update = new Task(() => GetUpdate());
                update.Start();
                update.Wait();
                appSettings.PreviosUpdate = new List<DateTime>() { _lastProjectsUpdate, _lastInvoicesUpdate }.Min(dt => dt);
                appSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(appSettings.SectionInformation.Name);
                await Task.Delay(delay);
            };
        }

        private static void GetUpdate()
        {
            var currentUpdate = DateTime.Now;

            if (TryGetData(_lastProjectsUpdate, currentUpdate, out List<Project> projects))
            {
                foreach (var project in projects)
                {
                    using var db = new Context();
                    project.MainProject = Project.GetOrAdd(project.MainProject, db);
                    project.Presale = Presale.GetOrAdd(project.Presale, db);
                    Project.UpdateActions(project, db);
                    Project.GetOrAdd(project, db);
                }
                _lastProjectsUpdate = currentUpdate;
            }

            if (TryGetData(_lastInvoicesUpdate, currentUpdate, out List<Invoice> invoices))
            {
                foreach (var invoice in invoices)
                {
                    using var db = new Context();
                    invoice.Project = Project.GetOrAdd(invoice.Project, db);
                    invoice.Presale = Presale.GetOrAdd(invoice.Presale, db);
                    Invoice.AddOrUpdate(invoice, db);
                }
                _lastInvoicesUpdate = currentUpdate;
            }
        }

        private static bool TryGetData<T>(DateTime startTime, DateTime endTime, out List<T> result)
        {
            if (Settings.TryGetSection<Settings.Connection>(
                    out ConfigurationSection? r) && r != null)
            {
                var connSettings = (Settings.Connection)r;
                _auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    $"{connSettings.Username}:{connSettings.Password}"));

                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri($"http://{connSettings.Url}");

                result = new List<T>();
                var request = $"trade/hs/API/Get{typeof(T).Name}s?" +
                    $"begin={startTime:yyyy-MM-ddTHH:mm:ss}" +
                    $"&end={endTime:yyyy-MM-ddTHH:mm:ss}";
                var message = new HttpRequestMessage(HttpMethod.Get, request);
                message.Headers.Add("Authorization", $"Basic {_auth}");

                Console.WriteLine($"[{DateTime.Now}] Request: {message.RequestUri}");
                Console.WriteLine($"[{DateTime.Now}] Waiting response...");
                var response = httpClient.SendAsync(message).Result;
                Console.WriteLine($"[{DateTime.Now}] Bytes received: {response.Content.Headers.ContentLength}");

                if (response.IsSuccessStatusCode)
                {
                    var objects = JsonConvert.DeserializeObject<dynamic>(
                        response.Content.ReadAsStringAsync().Result, deserializeSettings);
                    if (objects != null)
                    {
                        foreach (var obj in objects)
                        {
                            result.Add(obj.ToObject<T>());
                        }
                        Console.WriteLine($"[{DateTime.Now}] {typeof(T).Name}s updated: {result.Count}");
                    }
                    return true;
                }
            }

            else throw new ConfigurationErrorsException() { };
            return false;
        }
    }
}

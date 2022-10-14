using Newtonsoft.Json;
using Entities;
using System.Configuration;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PresalesStatistic
{
    public class Parser
    {
        private static readonly FileInfo _errorLog = new("Error.log");
        private static readonly FileInfo _workLog = new("Parser.log");
        private static DateTime _lastProjectsUpdate;
        private static DateTime _lastInvoicesUpdate;
        private static string? _auth;
        private static readonly JsonSerializerSettings deserializeSettings = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        public static void Run(DbContextOptions<DbController.Context> dbOptions)
        {
            Settings.TryGetSection<Settings.Application>(out ConfigurationSection? r);
            if (r == null) return;
            var appSettings = (Settings.Application)r;

            _lastProjectsUpdate = appSettings.PreviosUpdate;
            _lastInvoicesUpdate = appSettings.PreviosUpdate;

            GetUpdate(dbOptions, appSettings.PreviosUpdate);
            appSettings.PreviosUpdate = new List<DateTime>() { _lastProjectsUpdate, _lastInvoicesUpdate }.Min(dt => dt);
            appSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(appSettings.SectionInformation.Name);
        }

        private static void GetUpdate(DbContextOptions<DbController.Context> dbOptions, DateTime prevUpdate)
        {
            try
            {
                var currentUpdate = (DateTime.Now - prevUpdate).TotalDays > 1 ? prevUpdate.AddDays(1) : DateTime.Now;
                if (TryGetData(_lastProjectsUpdate, currentUpdate, out List<Project> projects))
                {
                    foreach (var project in projects)
                    {
                        using var db = new DbController.Context(dbOptions);
                        project.MainProject = project.MainProject.GetOrAdd(db);
                        project.Presale = project.Presale.GetOrAdd(db);
                        project.AddOrUpdate(db);
                    }
                    _lastProjectsUpdate = currentUpdate;
                }

                if (TryGetData(_lastInvoicesUpdate, currentUpdate, out List<Invoice> invoices))
                {
                    foreach (var invoice in invoices)
                    {
                        using var db = new DbController.Context(dbOptions);
                        invoice.Presale = invoice.Presale.GetOrAdd(db);
                        invoice.Project = invoice.Project.GetOrAdd(db);
                        if (invoice.Presale != null && invoice.Project != null)
                            invoice.Project.Presale = invoice.Presale;
                        invoice.AddOrUpdate(db, out bool isNew);
                        if (isNew == false)
                        {
                            using var sw = File.AppendText(_workLog.FullName);
                            sw.WriteLine($"\t{invoice.Number} updated!");
                        }
                    }
                    _lastInvoicesUpdate = currentUpdate;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                using var sw = File.AppendText(_errorLog.FullName);
                sw.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] {e}\n");
            }
        }

        private static bool TryGetData<T>(DateTime startTime, DateTime endTime, out List<T> result)
        {
            result = new List<T>();
            if (Settings.TryGetSection<Settings.Connection>(
                    out ConfigurationSection? r) && r != null)
            {
                var connSettings = (Settings.Connection)r;
                _auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    $"{connSettings.Username}:{connSettings.Password}"));

                var httpClient = new HttpClient { BaseAddress = new Uri($"http://{connSettings.Url}") };

                var request = $"trade/hs/API/Get{typeof(T).Name}s?" +
                    $"begin={startTime:yyyy-MM-ddTHH:mm:ss}" +
                    $"&end={endTime:yyyy-MM-ddTHH:mm:ss}";
                var message = new HttpRequestMessage(HttpMethod.Get, request);
                message.Headers.Add("Authorization", $"Basic {_auth}");

                // Console.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Request: {message.RequestUri}");
                using var sw = File.AppendText(_workLog.FullName);
                sw.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Request: {message.RequestUri}");
                // Console.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Waiting response...");
                var response = httpClient.SendAsync(message).Result;
                // Console.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Bytes received: {response.Content.Headers.ContentLength}");

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
                        // Console.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] {typeof(T).Name}s updated: {result.Count}");
                    }
                    return true;
                }
            }
            else throw new ConfigurationErrorsException() { };
            return false;
        }
    }
}

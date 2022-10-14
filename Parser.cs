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
        private static readonly FileInfo _errorLog = new("Error.log");
        private static readonly FileInfo _workLog = new("Parser.log");
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
                var update = new Task(() => GetUpdate(appSettings.PreviosUpdate));
                update.Start();
                update.Wait();
                appSettings.PreviosUpdate = new List<DateTime>() { _lastProjectsUpdate, _lastInvoicesUpdate }.Min(dt => dt);
                appSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(appSettings.SectionInformation.Name);
                Program.ShowData(appSettings.PreviosUpdate);
                await Task.Delay(delay);
            };
        }

        private static void GetUpdate(DateTime prevUpdate)
        {
            try
            {
                var currentUpdate = (DateTime.Now - prevUpdate).TotalDays > 1 ? prevUpdate.AddDays(1) : DateTime.Now;
                if (TryGetData(_lastProjectsUpdate, currentUpdate, out List<Project> projects))
                {
                    foreach (var project in projects)
                    {
                        using var db = new Context();
                        project.MainProject = Project.GetOrAdd(project.MainProject, db);
                        project.Presale = Presale.GetOrAdd(project.Presale, db);
                        Project.AddOrUpdate(project, db);
                    }
                    _lastProjectsUpdate = currentUpdate;
                }

                if (TryGetData(_lastInvoicesUpdate, currentUpdate, out List<Invoice> invoices))
                {
                    foreach (var invoice in invoices)
                    {
                        using var db = new Context();
                        invoice.Presale = Presale.GetOrAdd(invoice.Presale, db);
                        invoice.Project = Project.GetOrAdd(invoice.Project, db);
                        if (invoice.Presale != null && invoice.Project != null)
                            invoice.Project.Presale = invoice.Presale;
                        Invoice.AddOrUpdate(invoice, db, out var isNew);
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

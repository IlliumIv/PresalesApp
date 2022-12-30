using Newtonsoft.Json;
using PresalesMonitor.Entities;
using System.Configuration;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PresalesMonitor
{
    public class Parser
    {
        private static DateTime lastError = DateTime.MinValue;
        private static readonly FileInfo _errorLog = new("Error.log");
        private static readonly FileInfo _workLog = new("Parser.log");
        private static bool _debug = false;
        private static DateTime _lastProjectsUpdate;
        private static DateTime _lastInvoicesUpdate;
        private static string? _auth;
        private static readonly JsonSerializerSettings deserializeSettings = new()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        /*
        public static void Run()
        {
            if ((DateTime.Now - lastError) < TimeSpan.FromMinutes(10)) { Task.Delay(600000); }
            Settings.TryGetSection<Settings.Application>(out ConfigurationSection? r);
            if (r == null) return;
            var appSettings = (Settings.Application)r;

            _lastProjectsUpdate = appSettings.PreviosUpdate;
            _lastInvoicesUpdate = appSettings.PreviosUpdate;
            _debug = appSettings.Debug;

            GetUpdate(appSettings.PreviosUpdate);
            appSettings.PreviosUpdate = new List<DateTime>() { _lastProjectsUpdate, _lastInvoicesUpdate }.Min(dt => dt);
            appSettings.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(appSettings.SectionInformation.Name);
        }
        //*/
        private static void GetUpdate(DateTime prevUpdate)
        {
            try
            {
                var currentUpdate = (DateTime.Now - prevUpdate).TotalDays > 1 ? prevUpdate.AddDays(1) : DateTime.Now;
                
                if (TryGetData(_lastProjectsUpdate, currentUpdate, out List<Project> projects))
                {
                    foreach (var project in projects)
                    {
                        using var db = new DbController.Context();
                        project.MainProject = project.MainProject.GetOrAdd(db);
                        project.Presale = project.Presale.GetOrAdd(db);
                        var pr = project.AddOrUpdate(db, out bool isNew);
                        if (_debug)
                        {
                            using var sw = File.AppendText(_workLog.FullName);
                            if (isNew) sw.Write($"\tAdd: ");
                            else sw.Write($"\tUpdate: ");
                            sw.WriteLine($"Код:{pr.Number}," +
                                $"Наименование:{pr.Name}," +
                                $"Потенциал:{pr.PotentialAmount}," +
                                $"Статус:{pr.Status}," +
                                $"ПричинаПроигрыша:{pr.LossReason}," +
                                $"ПлановаяДатаОкончанияТек:{pr.PotentialWinAt}," +
                                $"ДатаОкончания:{pr.ClosedAt}," +
                                $"ДатаСогласованияРТС:{pr.ApprovalByTechDirectorAt}," +
                                $"ДатаСогласованияРОП:{pr.ApprovalBySalesDirectorAt}," +
                                $"ДатаНачалаРаботыПресейла:{pr.PresaleStartAt}," +
                                $"ДействияПресейла:{pr.Actions?.Count}," +
                                $"Пресейл:{pr.Presale?.Name}," +
                                $"ОсновнойПроект:{pr.MainProject?.Name}");
                        }
                    }
                    _lastProjectsUpdate = currentUpdate;
                }
                //*/

                // В 1С есть кеш, значения прибыли в счетах пересчитываются каждые :00 для трейда и каждые :10 для сателлита.
                // Если запрашивать актуальные изменения, то Прибыль придет 0, поэтому запрашиваем изменения за период полуторачасовой давности.
                if (TryGetData(_lastInvoicesUpdate.AddHours(-1.5), currentUpdate.AddHours(-1.5), out List<Invoice> invoices))
                {
                    foreach (var invoice in invoices)
                    {
                        using var db = new DbController.Context();
                        invoice.Presale = invoice.Presale.GetOrAdd(db);
                        invoice.Project = invoice.Project.GetOrAdd(db);
                        if (invoice.Presale != null && invoice.Project != null)
                            invoice.Project.Presale = invoice.Presale;
                        var inv = invoice.AddOrUpdate(db, out bool isNew);
                        if (_debug)
                        {
                            using var sw = File.AppendText(_workLog.FullName);
                            if (isNew) sw.Write($"\tAdd: ");
                            else sw.Write($"\tUpdate: ");

                            string profitPeriods = string.Empty;

                            if (inv.ProfitPeriods!= null)
                            {
                                foreach (var p in inv.ProfitPeriods)
                                    profitPeriods += $"\"{p.StartTime}\":\"{p.Amount}\",";
                            }

                            sw.WriteLine($"\"Номер\":\"{inv.Number}\"," +
                                $"\"Дата\":\"{inv.Date}\"," +
                                $"\"Контрагент\":\"{inv.Counterpart}\"," +
                                $"\"Проект\":\"{inv.Project?.Number}\"," +
                                $"\"СуммаРуб\":\"{inv.Amount}\"," +
                                $"\"ДатаПоследнейОплаты\":\"{inv.LastPayAt}\"," +
                                $"\"ДатаПоследнейОтгрузки\":\"{inv.LastShipmentAt}\"," +
                                $"\"Пресейл\":\"{inv.Presale?.Name}\"," +
                                $"\"Суммарная прибыль за периоды\":[{profitPeriods}]"
                                );
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
                lastError = DateTime.Now;
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

                using var sw = File.AppendText(_workLog.FullName);
                sw.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] Request: {message.RequestUri}");
                HttpResponseMessage response = httpClient.SendAsync(message).Result;

                if (response.IsSuccessStatusCode)
                {
                    var objects = JsonConvert.DeserializeObject<dynamic>(
                        response.Content.ReadAsStringAsync().Result, deserializeSettings);
                    if (objects != null)
                        foreach (var obj in objects)
                            result.Add(obj.ToObject<T>());
                    return true;
                }
                else throw new Exception($"{response.StatusCode}\n{response.Content.ReadAsStringAsync().Result}");
            }
            else throw new ConfigurationErrorsException() { };
        }
    }
}

using PresalesMonitor.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Configuration;
using PresalesMonitor.Entities;

namespace PresalesMonitor.Console
{
    public class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        static void Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
            };

            // using var db = new DbController.Context();
            // db.Delete();
            // db.Create();

            if (!Settings.ConfigurationFileIsExists()) Settings.CreateConfigurationFile();

            while (true)
            {
                Parser.Run();
                Settings.TryGetSection<Settings.Application>(out ConfigurationSection r);
                var _lastProjectsUpdate = ((Settings.Application)r).ProjectsUpdatedAt;
                var _lastInvoicesUpdate = ((Settings.Application)r).InvoicesUpdatedAt;
                ShowData(_lastProjectsUpdate, _lastInvoicesUpdate);
                int delay = 600000;
                if (DateTime.Now - new List<DateTime>() { _lastProjectsUpdate, _lastInvoicesUpdate }.Min(dt => dt) 
                    > TimeSpan.FromMilliseconds(delay)) delay = 0;
                Task.Delay(delay).Wait();
            };
        }

        public static void ShowData(DateTime ProjectsUpdatedAt, DateTime InvoicesUpdatedAt)
        {
            System.Console.WriteLine($"Последнее обновление проектов: {ProjectsUpdatedAt.AddMinutes(-2):dd.MM.yyyy HH:mm:ss.fff}");
            System.Console.WriteLine($"Последнее обновление счетов: {InvoicesUpdatedAt.AddHours(-1.5):dd.MM.yyyy HH:mm:ss.fff}");
        }
    }
}
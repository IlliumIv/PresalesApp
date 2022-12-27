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
                var prevUpdate = ((Settings.Application)r).PreviosUpdate;
                ShowData(prevUpdate);
                int delay = 600000;
                if (DateTime.Now - prevUpdate > TimeSpan.FromMilliseconds(delay)) delay = 0;
                Task.Delay(delay).Wait();
            };
        }

        public static void ShowData(DateTime prevUpdate)
        {
            System.Console.WriteLine($"\nДоступны данные за период: 20.09.2022 00:00:00 - {prevUpdate:dd.MM.yyyy HH:mm:ss}");
            System.Console.WriteLine($"Последнее обновление: {prevUpdate:dd.MM.yyyy HH:mm:ss.fff}");
        }
    }
}
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
            int delay = args.Length > 0 && int.TryParse(args[0], out delay) ? delay : 600000;

            while (true)
            {
                Parser.Run(delay);
            };
        }
    }
}
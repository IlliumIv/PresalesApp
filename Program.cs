using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PresalesStatistic.Entities;
using PresalesStatistic.Helpers;
using System.Configuration;

namespace PresalesStatistic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    ContractResolver = new JsonPropertiesResolver(),
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };
            };

            if (!Settings.ConfigurationFileIsExists()) Settings.CreateConfigurationFile();

            // using var db = new Context();
            // db.Delete();
            // db.Create();

            var parser = Parser.RunAsync();
            parser.Wait();

            // var presale = new Presale("Ivan", db);
            // var project = new Project("ЦБ-00000000");
            // _ = db.Presales.AddAsync(presale).Result;
            // _ = db.Projects.AddAsync(project).Result;
            // project.AssignPresale(presale);
            // db.SaveChanges();
            // var projects = db.Projects.ToListAsync().Result;
            // Console.WriteLine($"Projects in the database ({projects.Count}):");
            // foreach (var item in projects) Console.WriteLine($"{JsonConvert.SerializeObject(item, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local })}");
            // Console.ReadLine();
        }
    }

    public class Context : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Context() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Settings.TryGetSection<Settings.Database>(
                out ConfigurationSection? r) && r != null)
            {
                var dbSettings = (Settings.Database)r;

                optionsBuilder
                    .UseNpgsql($"host={dbSettings.Url};" +
                    $"port={dbSettings.Port};" +
                    $"database={dbSettings.DatabaseName};" +
                    $"username={dbSettings.Username};" +
                    $"password={dbSettings.Password}")
                    // .EnableSensitiveDataLogging().LogTo(message => Debug.WriteLine(message))
                    ;
            }
            else throw new ConfigurationErrorsException() { };
        }

        public DbSet<Presale> Presales { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PresaleAction> Actions { get; set; }
        public void Create() => Database.EnsureCreated();
        public void Delete() => Database.EnsureDeleted();
        public bool CanConnect() => Database.CanConnect();
        public bool Exists() => Database.GetService<IRelationalDatabaseCreator>().Exists();
    }
}
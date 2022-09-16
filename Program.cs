using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PresalesStatistic.Entities;
using PresalesStatistic.Helpers;
using System.Diagnostics;

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

            Parser parser = new Parser();
            using (var db = new Context())
            {
                parser.GetUpdate(db);
                // var presale = new Presale("Ivan", db);
                // var project = new Project("ЦБ-00000000");
                // _ = db.Presales.AddAsync(presale).Result;
                // _ = db.Projects.AddAsync(project).Result;
                // project.AssignPresale(presale);
                // db.SaveChanges();
                var projects = db.Projects.ToListAsync().Result;
                Console.WriteLine($"Projects in the database ({projects.Count}):");


                // foreach (var item in projects) Console.WriteLine($"{JsonConvert.SerializeObject(item, new JsonSerializerSettings() { DateTimeZoneHandling = DateTimeZoneHandling.Local })}");
            }
            // Console.ReadLine();
        }
    }

    public class Context : DbContext
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Context()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            // Database.EnsureDeleted();
            // Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(@"host=localhost;port=5432;database=presalesDb;username=presales;password=12345")
                // .EnableSensitiveDataLogging().LogTo(message => Debug.WriteLine(message))
                ;
        }

        public DbSet<Presale> Presales { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet <PresaleAction> Actions { get; set; }
    }
}
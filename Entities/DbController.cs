using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PresalesStatistic;
using System.Configuration;

namespace Entities
{
    public static class DbController
    {
        public class Context : DbContext
        {
            public Context() { }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!Settings.ConfigurationFileIsExists()) Settings.CreateConfigurationFile();

                Settings.TryGetSection<Settings.Database>(out ConfigurationSection? r);
                if (r == null) throw new ConfigurationErrorsException() { };
                var dbSettings = (Settings.Database)r;
                optionsBuilder.UseNpgsql($"host={dbSettings.Url};" +
                    $"port={dbSettings.Port};" +
                    $"database={dbSettings.DatabaseName};" +
                    $"username={dbSettings.Username};" +
                    $"password={dbSettings.Password}")
                    // .EnableSensitiveDataLogging().LogTo(message => Debug.WriteLine(message))
                    ;
            }
            public DbSet<Presale> Presales { get; set; }
            public DbSet<Project> Projects { get; set; }
            public DbSet<Invoice> Invoices { get; set; }
            public DbSet<PresaleAction> Actions { get; set; }
            public DbSet<ProfitDelta> ProfitDeltas { get; set; }
            public void Create() => Database.EnsureCreated();
            public void Delete() => Database.EnsureDeleted();
            public bool CanConnect() => Database.CanConnect();
            public bool Exists() => Database.GetService<IRelationalDatabaseCreator>().Exists();
        }

        public static Project AddOrUpdate(this Project project, Context db, out bool isNew)
        {
            isNew = true;
            var pr = db.Projects
                .Where(p => p.Number == project.Number)
                .FirstOrDefault();
            if (pr != null)
            {
                isNew = false;
                pr.Name = project.Name;
                pr.PotentialAmount = project.PotentialAmount;
                pr.Status = project.Status;
                pr.LossReason = project.LossReason;
                pr.ApprovalByTechDirectorAt = project.ApprovalByTechDirectorAt;
                pr.ApprovalBySalesDirectorAt = project.ApprovalBySalesDirectorAt;
                pr.PresaleStartAt = project.PresaleStartAt;
                pr.Presale = project.Presale;
                pr.MainProject = project.MainProject;
                pr.ClosedAt = project.ClosedAt;
                pr.PotentialWinAt = project.PotentialWinAt;
                pr.DeleteActions(db);
                pr.Actions = project.Actions;
                project = pr;
            }
            else
            {
                db.Projects.Add(project);
            }
            db.SaveChanges();
            return project;
        }

        public static Invoice AddOrUpdate(this Invoice invoice, Context db, out bool isNew)
        {
            isNew = true;
            var inv = db.Invoices
                .Where(i => i.Number == invoice.Number && i.Date.Date == invoice.Date.Date
                || i.Number == invoice.Number && i.Counterpart == invoice.Counterpart)
                .FirstOrDefault();
            if (inv != null)
            {
                isNew = false;
                inv.Counterpart = invoice.Counterpart;
                inv.Date = invoice.Date;
                inv.Amount = invoice.Amount;
                inv.Profit = invoice.Profit;
                inv.Presale = invoice.Presale;
                inv.Project = invoice.Project;
                inv.LastPayAt = invoice.LastPayAt;
                inv.LastShipmentAt = invoice.LastShipmentAt;
                if (invoice.ProfitDelta != 0) inv.ProfitDeltas.Add(new(invoice.ProfitDelta));
                invoice = inv;
            }
            else
            {
                db.Invoices.Add(invoice);
            }
            db.SaveChanges();
            return invoice;
        }

        public static Project? GetOrAdd(this Project? project, Context db)
        {
            if (project != null)
            {
                var pr = db.Projects
                    .Where(p => p.Number == project.Number)
                    .FirstOrDefault();
                if (pr != null) return pr;
                else
                {
                    db.Projects.Add(project);
                    db.SaveChanges();
                }
            }
            return project;
        }

        public static Presale? GetOrAdd(this Presale? presale, Context db)
        {
            if (presale != null)
            {
                var pr = db.Presales
                    .Where(p => p.Name == presale.Name)
                    .FirstOrDefault();
                if (pr != null) return pr;
                else
                {
                    db.Presales.Add(presale);
                    db.SaveChanges();
                }
            }
            return presale;
        }

        public static void DeleteActions(this Project project, Context db)
        {
            var proj = db.Projects
                .Where(p => p.Number == project.Number)
                .FirstOrDefault();
            if (proj != null)
            {
                var actions = db.Actions.Where(a => a.Project == proj).ToList();
                foreach (var action in actions) db.Actions.Remove(action);
            };
        }
    }
}

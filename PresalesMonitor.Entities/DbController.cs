using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Configuration;

namespace PresalesMonitor.Entities
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
            public DbSet<ProfitPeriod> ProfitPeriods { get; set; }
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
                .Include(p => p.Actions)
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
                pr.Actions = Update(pr.Actions, project.Actions, db);
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
                .Include(i => i.ProfitPeriods)
                .FirstOrDefault();
            if (inv != null)
            {
                isNew = false;
                inv.Date = invoice.Date;
                inv.Counterpart = invoice.Counterpart;
                inv.Project = invoice.Project;
                inv.Amount = invoice.Amount;
                inv.LastPayAt = invoice.LastPayAt;
                inv.LastShipmentAt = invoice.LastShipmentAt;
                inv.Presale = invoice.Presale;
                inv.ProfitPeriods = Update(inv.ProfitPeriods, invoice.ProfitPeriods, db);
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
        private static List<PresaleAction>? Update(List<PresaleAction>? items, List<PresaleAction>? newItems, Context db)
        {
            if (newItems == null) return null;
            if (items != null)
                foreach (var item in items)
                {
                    var equalItem = newItems.FirstOrDefault(i => i.Equals(item));
                    if (equalItem == null)
                    {
                        db.Remove(item);
                        continue;
                    }
                    newItems.Remove(equalItem);
                    newItems.Add(item);
                }
            return newItems;
        }
        private static List<ProfitPeriod>? Update(List<ProfitPeriod>? items, List<ProfitPeriod>? newItems, Context db)
        {
            if (newItems == null) return null;
            if (items != null)
                foreach (var item in items)
                {
                    var equalItem = newItems.FirstOrDefault(i => i.Equals(item));
                    if (equalItem == null)
                    {
                        db.Remove(item);
                        continue;
                    }
                    newItems.Remove(equalItem);
                    newItems.Add(item);
                }
            return newItems;
        }
    }
}

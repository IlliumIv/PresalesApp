using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Entities
{
    public static class DbController
    {
        public class Context : DbContext
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public Context(DbContextOptions<Context> options) : base(options) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            public DbSet<Presale> Presales { get; set; }
            public DbSet<Project> Projects { get; set; }
            public DbSet<Invoice> Invoices { get; set; }
            public DbSet<PresaleAction> Actions { get; set; }
            public void Create() => Database.EnsureCreated();
            public void Delete() => Database.EnsureDeleted();
            public bool CanConnect() => Database.CanConnect();
            public bool Exists() => Database.GetService<IRelationalDatabaseCreator>().Exists();
        }

        public static void AddOrUpdate(this Project project, Context db)
        {
            var pr = db.Projects.Where(p => p.Number == project.Number).FirstOrDefault();
            if (pr != null)
            {
                pr.Name = project.Name;
                pr.PotentialAmount = project.PotentialAmount;
                pr.LossReason = project.LossReason;
                pr.ApprovalByTechDirector = project.ApprovalByTechDirector;
                pr.ApprovalBySalesDirector = project.ApprovalBySalesDirector;
                pr.PresaleStart = project.PresaleStart;
                pr.DeleteActions(db);
                pr.Actions = project.Actions;
                pr.Presale = project.Presale;
                pr.MainProject = project.MainProject;
                if (pr.Status != project.Status)
                {
                    pr.Status = project.Status;
                    pr.LastStatusChanged = DateTime.UtcNow;
                }
            }
            else
            {
                db.Projects.Add(project);
            }
            db.SaveChanges();
        }

        public static void AddOrUpdate(this Invoice invoice, Context db, out bool isNew)
        {
            isNew = true;
            var inv = db.Invoices
                .Where(i => i.Number == invoice.Number && i.Data == invoice.Data)
                .FirstOrDefault();
            if (inv != null)
            {
                isNew = false;
                inv.Counterpart = invoice.Counterpart;
                inv.Amount = invoice.Amount;
                inv.Profit = invoice.Profit;
                inv.Presale = invoice.Presale;
                inv.Project = invoice.Project;
                inv.LastPay = invoice.LastPay;
                inv.LastShipment = invoice.LastShipment;
            }
            else
            {
                db.Invoices.Add(invoice);
            }
            db.SaveChanges();
        }

        public static Project? GetOrAdd(this Project? project, Context db)
        {
            if (project != null)
            {
                var pr = db.Projects.Where(p => p.Number == project.Number).FirstOrDefault();
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
                var pr = db.Presales.Where(p => p.Name == presale.Name).FirstOrDefault();
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
            var proj = db.Projects.Where(p => p.Number == project.Number).FirstOrDefault();
            if (proj != null)
            {
                var actions = db.Actions.Where(a => a.Project == proj).ToList();
                foreach (var action in actions) db.Actions.Remove(action);
            };
        }
    }
}

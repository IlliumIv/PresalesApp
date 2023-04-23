using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;

namespace PresalesApp.Database
{
    public static class DbController
    {
        internal static readonly QueriesQueue<Task> Queries = new();
        private readonly static ManualResetEvent _while_queue_empty = new(false);

        public static void Start(ILogger logger)
        {
            Log.Logger = logger;

            Queries.OnEnqueued += Queries_OnEnqueued;
            Queries.OnReachedEmpty += Queries_OnReachedEmpty;

            new Task(() =>
            {
                while (true)
                {
                    if (Queries.Any())
                    {
                        var query = Queries.Dequeue();
                        query?.Start();
                        query?.Wait();
                    }
                    else
                        _while_queue_empty.WaitOne();
                }
            }).Start();

            Log.Information($"{typeof(DbController).FullName} started.");
        }

        private static void Queries_OnReachedEmpty(object? sender, EventArgs e)
        {
            _while_queue_empty.Reset();
        }

        private static void Queries_OnEnqueued(object? sender, EventArgs e)
        {
            _while_queue_empty.Set();
        }

        internal static List<T>? Update<T>(this List<T>? itemsA, List<T>? itemsB, ReadWriteContext dbContext)
            where T : Entity
        {
            if (itemsB == null) return null;
            if (itemsA != null)
                foreach (var item in itemsA)
                {
                    var equal_item = itemsB.FirstOrDefault(i => i.Equals(item));

                    if (equal_item == null)
                    {
                        dbContext.Remove(item);
                        continue;
                    }
                    itemsB.Remove(equal_item);
                    itemsB.Add(item);
                }

            return itemsB;
        }

        internal class ReadWriteContext : ReadOnlyContext
        {
            public ReadWriteContext() { }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql($"host={Settings.Default.Host};" +
                    $"port={Settings.Default.Port};" +
                    $"database={Settings.Default.Database};" +
                    $"username={Settings.Default.Username};" +
                    $"password={Settings.Default.Password}");
            }

            public new int SaveChanges() => base.BaseSaveChanges();

            public new int SaveChanges(bool acceptAll) => base.BaseSaveChanges(acceptAll);

            public new Task<int> SaveChangesAsync(CancellationToken token = default)
                => base.BaseSaveChangesAsync(token);

            public new Task<int> SaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
                base.BaseSaveChangesAsync(acceptAll, token);

            public void Create() => Database.EnsureCreated();
            public void Delete() => Database.EnsureDeleted();
        }

        public class ReadOnlyContext : IdentityDbContext<User>
        {
            // https://stackoverflow.com/a/10438977
            public ReadOnlyContext() { }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql($"host={Settings.Default.Host};" +
                    $"port={Settings.Default.Port};" +
                    $"database={Settings.Default.Database};" +
                    $"username={Settings.Default.Username};" +
                    $"password={Settings.Default.Password};" +
                    $"options=-c default_transaction_read_only=on")
                    // .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    ;
            }

            public DbSet<Presale> Presales { get; set; }
            public DbSet<Project> Projects { get; set; }
            public DbSet<Invoice> Invoices { get; set; }
            public DbSet<ProfitPeriod> ProfitPeriods { get; set; }
            public DbSet<PresaleAction> PresaleActions { get; set; }
            public DbSet<CacheLog> CacheLogsHistory { get; set; }
            public DbSet<ProjectsUpdate> ProjectsUpdates { get; set; }
            public DbSet<CacheLogsUpdate> CacheLogsUpdates { get; set; }
            public DbSet<Update> Updates { get; set; }

            [Obsolete("This context is read-only", true)]
            public new int SaveChanges() =>
                throw new InvalidOperationException("This context is read-only.");

            [Obsolete("This context is read-only", true)]
            public new int SaveChanges(bool acceptAll) =>
                throw new InvalidOperationException("This context is read-only.");

            [Obsolete("This context is read-only", true)]
            public new Task<int> SaveChangesAsync(CancellationToken token = default) =>
                throw new InvalidOperationException("This context is read-only.");

            [Obsolete("This context is read-only", true)]
            public new Task<int> SaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
                throw new InvalidOperationException("This context is read-only.");

            internal int BaseSaveChanges() => base.SaveChanges();

            internal int BaseSaveChanges(bool acceptAll) => base.SaveChanges(acceptAll);

            internal Task<int> BaseSaveChangesAsync(CancellationToken token = default) => base.SaveChangesAsync(token);

            internal Task<int> BaseSaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
                base.SaveChangesAsync(acceptAll, token);
        }

        public class TODOReadWriteContext : ReadOnlyContext
        {
            public TODOReadWriteContext() { }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql($"host={Settings.Default.Host};" +
                    $"port={Settings.Default.Port};" +
                    $"database={Settings.Default.Database};" +
                    $"username={Settings.Default.Username};" +
                    $"password={Settings.Default.Password}");
            }

            public new int SaveChanges() => base.BaseSaveChanges();

            public new int SaveChanges(bool acceptAll) => base.BaseSaveChanges(acceptAll);

            public new Task<int> SaveChangesAsync(CancellationToken token = default)
                => base.BaseSaveChangesAsync(token);

            public new Task<int> SaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
                base.BaseSaveChangesAsync(acceptAll, token);

            public void Create() => Database.EnsureCreated();
            public void Delete() => Database.EnsureDeleted();
        }
    }
}

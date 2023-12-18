using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PresalesApp.Database.Entities;
using PresalesApp.Database.Entities.Updates;
using Serilog;

namespace PresalesApp.Database;

public static class DbController
{
    internal static readonly QueriesQueue<Task> Queries = new();

    private readonly static ManualResetEvent _WhileQueueEmpty = new(false);

    internal static Settings? DbSettings { get; private set; }

    public static void Configure (Settings settings) => DbSettings = settings;

    public static void Start()
    {
        Queries.OnEnqueued += _Queries_OnEnqueued;
        Queries.OnReachedEmpty += _Queries_OnReachedEmpty;

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
                else _WhileQueueEmpty.WaitOne();
            }
        }).Start();

        Log.Information($"{typeof(DbController).FullName} started.");
    }

    private static void _Queries_OnReachedEmpty(object? sender, EventArgs e) =>
        _WhileQueueEmpty.Reset();

    private static void _Queries_OnEnqueued(object? sender, EventArgs e) =>
        _WhileQueueEmpty.Set();

    internal class ControllerContext : ReadOnlyContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql($"host={DbSettings!.DbHost};" +
                $"port={DbSettings.DbPort};" +
                $"database={DbSettings.DbName};" +
                $"username={DbSettings.DbUsername};" +
                $"password={DbSettings.DbPassword}");

        public new int SaveChanges() => BaseSaveChanges();

        public new int SaveChanges(bool acceptAll) => BaseSaveChanges(acceptAll);

        public new Task<int> SaveChangesAsync(CancellationToken token = default) =>
            BaseSaveChangesAsync(token);

        public new Task<int> SaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
            BaseSaveChangesAsync(acceptAll, token);
    }

    public class ReadOnlyContext : UsersContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"host={DbSettings!.DbHost};" +
                $"port={DbSettings.DbPort};" +
                $"database={DbSettings.DbName};" +
                $"username={DbSettings.DbUsername};" +
                $"password={DbSettings.DbPassword};" +
                $"options=-c default_transaction_read_only=on")
                // .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                ;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                optionsBuilder.LogTo(Log.Logger.Information, LogLevel.Information, null)
                .EnableSensitiveDataLogging();
            }
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

        // https://stackoverflow.com/a/10438977
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

        internal Task<int> BaseSaveChangesAsync(CancellationToken token = default) =>
            base.SaveChangesAsync(token);

        internal Task<int> BaseSaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
            base.SaveChangesAsync(acceptAll, token);
    }

    public class UsersContext : IdentityDbContext<User>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql($"host={DbSettings!.DbHost};" +
                $"port={DbSettings.DbPort};" +
                $"database={DbSettings.DbName};" +
                $"username={DbSettings.DbUsername};" +
                $"password={DbSettings.DbPassword}");
    }
}

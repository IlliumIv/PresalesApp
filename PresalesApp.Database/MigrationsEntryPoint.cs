using Microsoft.EntityFrameworkCore;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database;

public class MigrationsEntryPoint
{
#pragma warning disable IDE0060 // Remove unused parameter
    static void Main(string[] args) { }
#pragma warning restore IDE0060 // Remove unused parameter

    internal class MigrationsContext : ReadOnlyContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql($"host=127.0.0.1;" +
                $"port=5432;" +
                $"database=presalesapp;" +
                $"username=presale;" +
                $"password=***REMOVED***");

        public new int SaveChanges() => BaseSaveChanges();

        public new int SaveChanges(bool acceptAll) => BaseSaveChanges(acceptAll);

        public new Task<int> SaveChangesAsync(CancellationToken token = default)
            => BaseSaveChangesAsync(token);

        public new Task<int> SaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
            BaseSaveChangesAsync(acceptAll, token);
    }
}

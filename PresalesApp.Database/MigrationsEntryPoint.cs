using Microsoft.EntityFrameworkCore;
using static PresalesApp.Database.DbController;

namespace PresalesApp.Database
{
    public class MigrationsEntryPoint
    {
        static void Main(string[] args) { }

        internal class MigrationsContext : ReadOnlyContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql($"host=;" +
                    $"port=;" +
                    $"database=;" +
                    $"username=;" +
                    $"password=");
            }

            public new int SaveChanges() => base.BaseSaveChanges();

            public new int SaveChanges(bool acceptAll) => base.BaseSaveChanges(acceptAll);

            public new Task<int> SaveChangesAsync(CancellationToken token = default)
                => base.BaseSaveChangesAsync(token);

            public new Task<int> SaveChangesAsync(bool acceptAll, CancellationToken token = default) =>
                base.BaseSaveChangesAsync(acceptAll, token);
        }
    }
}

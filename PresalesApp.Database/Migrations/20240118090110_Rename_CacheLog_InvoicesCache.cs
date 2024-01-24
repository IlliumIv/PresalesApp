using Microsoft.EntityFrameworkCore.Migrations;
using PresalesApp.Database.Entities.Updates;

#nullable disable

namespace PresalesApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class Rename_CacheLog_InvoicesCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE public.\"Updates\" SET \"Discriminator\" = 'InvoicesCache' WHERE \"Discriminator\" = 'CacheLog';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE public.\"Updates\" SET \"Discriminator\" = 'CacheLog' WHERE \"Discriminator\" = 'InvoicesCache';");
        }
    }
}

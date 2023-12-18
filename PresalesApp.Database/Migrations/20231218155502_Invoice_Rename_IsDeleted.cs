using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations;

/// <inheritdoc />
public partial class Invoice_Rename_IsDeleted : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.RenameColumn(
            name: "IsDeleted",
            table: "Invoices",
            newName: "MarkedAsDeleted");

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.RenameColumn(
            name: "MarkedAsDeleted",
            table: "Invoices",
            newName: "IsDeleted");
}

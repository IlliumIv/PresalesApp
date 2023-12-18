using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations;

/// <inheritdoc />
public partial class AddPresaleStatus : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Presales",
            type: "boolean",
            nullable: false,
            defaultValue: false);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Presales");
}

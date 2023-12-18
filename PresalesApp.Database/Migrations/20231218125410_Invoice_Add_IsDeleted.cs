using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations;

/// <inheritdoc />
public partial class Invoice_Add_IsDeleted : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Discriminator",
            table: "Updates",
            type: "character varying(21)",
            maxLength: 21,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Invoices",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Invoices");

        migrationBuilder.AlterColumn<string>(
            name: "Discriminator",
            table: "Updates",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(21)",
            oldMaxLength: 21);
    }
}

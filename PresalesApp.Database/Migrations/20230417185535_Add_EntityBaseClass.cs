using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations;

/// <inheritdoc />
public partial class Add_EntityBaseClass : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Projects_Projects_MainProjectProjectId",
            table: "Projects");

        migrationBuilder.RenameColumn(
            name: "PerionEnd",
            table: "Updates",
            newName: "PeriodEnd");

        migrationBuilder.RenameColumn(
            name: "MainProjectProjectId",
            table: "Projects",
            newName: "MainProjectId");

        migrationBuilder.RenameColumn(
            name: "ProjectId",
            table: "Projects",
            newName: "Id");

        migrationBuilder.RenameIndex(
            name: "IX_Projects_MainProjectProjectId",
            table: "Projects",
            newName: "IX_Projects_MainProjectId");

        migrationBuilder.RenameColumn(
            name: "ProfitPeriodId",
            table: "ProfitPeriods",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "PresaleId",
            table: "Presales",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "PresaleActionId",
            table: "PresaleActions",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "InvoiceId",
            table: "Invoices",
            newName: "Id");

        migrationBuilder.AlterColumn<DateTime>(
            name: "SynchronizedTo",
            table: "Updates",
            type: "timestamp with time zone",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Projects_Projects_MainProjectId",
            table: "Projects",
            column: "MainProjectId",
            principalTable: "Projects",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Projects_Projects_MainProjectId",
            table: "Projects");

        migrationBuilder.RenameColumn(
            name: "PeriodEnd",
            table: "Updates",
            newName: "PerionEnd");

        migrationBuilder.RenameColumn(
            name: "MainProjectId",
            table: "Projects",
            newName: "MainProjectProjectId");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "Projects",
            newName: "ProjectId");

        migrationBuilder.RenameIndex(
            name: "IX_Projects_MainProjectId",
            table: "Projects",
            newName: "IX_Projects_MainProjectProjectId");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "ProfitPeriods",
            newName: "ProfitPeriodId");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "Presales",
            newName: "PresaleId");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "PresaleActions",
            newName: "PresaleActionId");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "Invoices",
            newName: "InvoiceId");

        migrationBuilder.AlterColumn<DateTime>(
            name: "SynchronizedTo",
            table: "Updates",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AddForeignKey(
            name: "FK_Projects_Projects_MainProjectProjectId",
            table: "Projects",
            column: "MainProjectProjectId",
            principalTable: "Projects",
            principalColumn: "ProjectId");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations;

/// <inheritdoc />
public partial class Add_FunnelStage : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<int>(
            name: "FunnelStage",
            table: "Projects",
            type: "integer",
            nullable: false,
            defaultValue: 0);

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(
            name: "FunnelStage",
            table: "Projects");
}

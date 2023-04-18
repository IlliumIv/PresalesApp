using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PresalesApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_TrackUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PresaleActions_Projects_ProjectId",
                table: "PresaleActions");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "PresaleActions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "Updates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    PeriodBegin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerionEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SynchronizedTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Updates", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PresaleActions_Projects_ProjectId",
                table: "PresaleActions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PresaleActions_Projects_ProjectId",
                table: "PresaleActions");

            migrationBuilder.DropTable(
                name: "Updates");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "PresaleActions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PresaleActions_Projects_ProjectId",
                table: "PresaleActions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

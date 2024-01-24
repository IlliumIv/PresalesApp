using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class Actions_Add_Rank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "PresaleActions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "PresaleActions");
        }
    }
}

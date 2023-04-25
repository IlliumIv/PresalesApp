using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations
{
    /// <inheritdoc />
    public partial class Rename_Actions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Actions_ProjectId",
                table: "Actions");

            migrationBuilder.DropForeignKey(
                name: "FK_Actions_Projects_ProjectId",
                table: "Actions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Actions",
                table: "Actions");

            migrationBuilder.RenameTable(
                name: "Actions",
                newName: "PresaleActions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PresaleActions",
                table: "PresaleActions",
                column: "PresaleActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PresaleActions_Projects_ProjectId",
                table: "PresaleActions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PresaleActions_ProjectId",
                table: "PresaleActions",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PresaleActions_ProjectId",
                table: "PresaleActions");

            migrationBuilder.DropForeignKey(
                name: "FK_PresaleActions_Projects_ProjectId",
                table: "PresaleActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PresaleActions",
                table: "PresaleActions");

            migrationBuilder.RenameTable(
                name: "PresaleActions",
                newName: "Actions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actions",
                table: "Actions",
                column: "PresaleActionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Actions_Projects_ProjectId",
                table: "Actions",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Actions_ProjectId",
                table: "Actions",
                column: "ProjectId");
        }
    }
}

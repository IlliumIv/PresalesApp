using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesMonitor.Entities.Migrations
{
    public partial class Project_Add_LastStatusChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChanged",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStatusChanged",
                table: "Projects");
        }
    }
}

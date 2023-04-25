using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PresalesApp.Database.Migrations
{
    public partial class UpdateInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPay",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastShipment",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPay",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LastShipment",
                table: "Invoices");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication.Migrations
{
    public partial class UpdatePizzasBis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "type",
                table: "Pizzas",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "rating",
                table: "Pizzas",
                newName: "Rating");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Pizzas",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Pizzas",
                newName: "rating");
        }
    }
}

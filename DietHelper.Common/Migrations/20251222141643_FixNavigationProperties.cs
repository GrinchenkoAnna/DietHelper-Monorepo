using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietHelper.Common.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Calories",
                table: "UserDishIngredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Carbs",
                table: "UserDishIngredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Fat",
                table: "UserDishIngredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Protein",
                table: "UserDishIngredients",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Calories",
                table: "UserDishIngredients");

            migrationBuilder.DropColumn(
                name: "Carbs",
                table: "UserDishIngredients");

            migrationBuilder.DropColumn(
                name: "Fat",
                table: "UserDishIngredients");

            migrationBuilder.DropColumn(
                name: "Protein",
                table: "UserDishIngredients");
        }
    }
}

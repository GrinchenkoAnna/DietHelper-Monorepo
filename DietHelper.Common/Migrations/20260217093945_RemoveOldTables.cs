using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietHelper.Common.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishIngredients",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Dishes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

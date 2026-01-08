using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietHelper.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadyDishToUserDish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReadyDish",
                table: "UserDishes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadyDish",
                table: "UserDishes");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietHelper.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeToBaseProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "BaseProducts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "BaseProducts");
        }
    }
}

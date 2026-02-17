using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DietHelper.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserMealEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserDishId = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalProtein = table.Column<double>(type: "double precision", nullable: false),
                    TotalFat = table.Column<double>(type: "double precision", nullable: false),
                    TotalCarbs = table.Column<double>(type: "double precision", nullable: false),
                    TotalCalories = table.Column<double>(type: "double precision", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMealEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMealEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMealEntries_UserDishes_UserDishId",
                        column: x => x.UserDishId,
                        principalTable: "UserDishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserMealEntryIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserMealEntryId = table.Column<int>(type: "integer", nullable: false),
                    UserProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "text", nullable: false),
                    ProteinSnapshot = table.Column<double>(type: "double precision", nullable: false),
                    FatSnapshot = table.Column<double>(type: "double precision", nullable: false),
                    CarbsSnapshot = table.Column<double>(type: "double precision", nullable: false),
                    CaloriesSnapshot = table.Column<double>(type: "double precision", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMealEntryIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMealEntryIngredients_UserMealEntries_UserMealEntryId",
                        column: x => x.UserMealEntryId,
                        principalTable: "UserMealEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserMealEntryIngredients_UserProducts_UserProductId",
                        column: x => x.UserProductId,
                        principalTable: "UserProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMealEntries_UserDishId",
                table: "UserMealEntries",
                column: "UserDishId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMealEntries_UserId_Date",
                table: "UserMealEntries",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_UserMealEntryIngredients_UserMealEntryId",
                table: "UserMealEntryIngredients",
                column: "UserMealEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMealEntryIngredients_UserProductId",
                table: "UserMealEntryIngredients",
                column: "UserProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMealEntryIngredients");

            migrationBuilder.DropTable(
                name: "UserMealEntries");
        }
    }
}

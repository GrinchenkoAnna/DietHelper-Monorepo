using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietHelper.Common.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //новый пользователь
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""PasswordHash"", ""Name"")
                VALUES ('system_hash', 'System User');
            ");

            //перенос продуктов из Products в BaseProducts
            migrationBuilder.Sql(@"
                INSERT INTO ""BaseProducts"" (""Name"", ""Protein"", ""Fat"", ""Carbs"", ""Calories"", ""IsDeleted"")
                SELECT ""Name"", ""Protein"", ""Fat"", ""Carbs"", ""Calories"", ""IsDeleted""
                FROM ""Products""
                WHERE ""IsDeleted"" = false;
            ");

            //заполнение UserProducts (1 продукт для примера)
            migrationBuilder.Sql(@"
                INSERT INTO ""UserProducts"" (""UserId"", ""BaseProductId"", ""CustomProtein"", ""CustomFat"", ""CustomCarbs"", ""CustomCalories"", ""IsDeleted"")
                SELECT 1, bp.""Id"", bp.""Protein"", bp.""Fat"", bp.""Carbs"", bp.""Calories"", false
                FROM ""BaseProducts"" bp;
            ");

            //перенос блюд из Dishes в UserDishes
            migrationBuilder.Sql(@"
                INSERT INTO ""UserDishes"" (""UserId"", ""Name"", ""Protein"", ""Fat"", ""Carbs"", ""Calories"", ""IsDeleted"")
                SELECT 1, d.""Name"", COALESCE(d.""Protein"", 0), COALESCE(d.""Fat"", 0), COALESCE(d.""Carbs"", 0), COALESCE(d.""Calories"", 0), d.""IsDeleted""
                FROM ""Dishes"" d
                WHERE d.""IsDeleted"" = false;
            ");

            //заполнение UserDishIngredients на основе DishIngredients
            migrationBuilder.Sql(@"
                INSERT INTO ""UserDishIngredients"" (""UserDishId"", ""UserProductId"", ""Quantity"", ""IsDeleted"")
                SELECT ud.""Id"", up.""Id"", di.""Quantity"", di.""IsDeleted""
                FROM ""DishIngredients"" di
                INNER JOIN ""Dishes"" d 
                    ON d.""Id"" = di.""DishId""
                INNER JOIN ""UserDishes"" ud 
                    ON ud.""Name"" = d.""Name"" AND ud.""UserId"" = 1
                INNER JOIN ""Products"" p 
                    ON p.""Id"" = di.""ProductId""
                INNER JOIN ""BaseProducts"" bp 
                    ON bp.""Name"" = p.""Name"" AND bp.""IsDeleted"" = false
                INNER JOIN ""UserProducts"" up 
                    ON up.""BaseProductId"" = bp.""Id"" AND up.""UserId"" = 1
                WHERE di.""IsDeleted"" = false;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""UserDishIngredients""");
            migrationBuilder.Sql(@"DELETE FROM ""UserDishes""");
            migrationBuilder.Sql(@"DELETE FROM ""UserProducts""");
            migrationBuilder.Sql(@"DELETE FROM ""BaseProducts""");
            migrationBuilder.Sql(@"DELETE FROM ""Users""");
        }
    }
}

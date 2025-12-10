using DietHelper.Common.Data;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public class DatabaseService
    {
        private DietHelperDbContext CreateContext()
        {
            return new DietHelperDbContext();
        }

        #region Mocks
        public async Task<Product?> GetProductMocksAsync()
        {
            var products = await GetProductsAsync();
            return products.FirstOrDefault();
        }

        public async Task<Dish?> GetDishMocksAsync()
        {
            var dishes = await GetDishesAsync();
            return dishes.FirstOrDefault();
        }
        #endregion

        #region Products
        public async Task<List<Product>> GetProductsAsync()
        {
            using var context = CreateContext();

            return await context.Products.Where(p => !p.IsDeleted).ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            using var context = CreateContext();

            return await context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Dictionary<int, Product>?> GetProductsByIdAsync(List<int> ids)
        {
            using var context = CreateContext();

            var products = await context.Products
                .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
                .ToDictionaryAsync(p => p.Id, p => p);

            return products;
        }

        public async Task<int> AddProductAsync(Product product)
        {
            using var context = CreateContext();

            product.IsDeleted = false;

            context.Products.Add(product);
            await context.SaveChangesAsync();
            return product.Id;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            using var context = CreateContext();
            var existingProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id && !p.IsDeleted);

            if (existingProduct is null) return false;

            existingProduct.Name = product.Name;
            existingProduct.NutritionFacts = product.NutritionFacts;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            using var context = CreateContext();

            var product = await context.Products.FindAsync(id);
            if (product is null || product.IsDeleted) return false;

            product.IsDeleted = true;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreProductAsync(int id)
        {
            using var context = CreateContext();

            var product = await context.Products.FindAsync(id);
            if (product is null || !product.IsDeleted) return false;

            product.IsDeleted = false;

            return await context.SaveChangesAsync() > 0;
        }
        #endregion

        #region Dishes
        public async Task<List<Dish>> GetDishesAsync()
        {
            using var context = CreateContext();

            return await context.Dishes
                .Where(d => !d.IsDeleted)
                .Include(d => d.Ingredients.Where(i => !i.IsDeleted))
                .ThenInclude(di => di.Ingredient)
                .ToListAsync();
        }

        public async Task<Dish?> GetDishByIdAsync(int id)
        {
            using var context = CreateContext();

            return await context.Dishes
                .Where(d => !d.IsDeleted)
                .Include(d => d.Ingredients)
                .ThenInclude(di => di.Ingredient)
                .Where(d => d.Ingredients.All(i => !i.IsDeleted))
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<int> AddDishAsync(Dish dish)
        {
            using var context = CreateContext();

            dish.IsDeleted = false;

            foreach (var ingredient in dish.Ingredients) ingredient.IsDeleted = false;

            //почему не надо: когда блюдо новое, в нем нет игредиентов,
            //и nutritionfacts пересчитываются по нулям,
            //стирая введенные пользователем данные
            //dish.UpdateNutritionFromIngredients();

            context.Dishes.Add(dish);
            await context.SaveChangesAsync();

            return dish.Id;
        }

        public async Task<bool> UpdateDishAsync(Dish dish)
        {
            using var context = CreateContext();

            var dishExists = await context.Dishes
                .AnyAsync(d => d.Id == dish.Id && !d.IsDeleted);
            if (!dishExists) return false;

            var existingDish = await context.Dishes
                .Include(d => d.Ingredients)
                .FirstOrDefaultAsync(d => d.Id == dish.Id && !d.IsDeleted);

            if (existingDish is null) return false;

            existingDish.Name = dish.Name;
            existingDish.NutritionFacts = dish.NutritionFacts;

            var existingIngredients = existingDish.Ingredients.Where(i => !i.IsDeleted).ToList();

            var newIngredientData = dish.Ingredients
                .Where(i => !i.IsDeleted)
                .Select(i => new { i.ProductId, i.Quantity })
                .ToList();

            //удалить ингредиенты, которых нет в новых данных
            foreach (var existingIngredient in existingIngredients)
            {
                if (!newIngredientData.Any(newIng => newIng.ProductId == existingIngredient.ProductId))
                {
                    existingIngredient.IsDeleted = true;
                }
            }

            foreach (var newIngredient in newIngredientData)
            {
                var existingIngredient = existingIngredients
                    .FirstOrDefault(i => i.ProductId == newIngredient.ProductId);

                if (existingIngredient is not null)
                {
                    existingIngredient.Quantity = newIngredient.Quantity;
                    existingIngredient.IsDeleted = false;
                }
                else
                {
                    var ingredientToAdd = new DishIngredient
                    {
                        DishId = existingDish.Id,
                        ProductId = newIngredient.ProductId,
                        Quantity = newIngredient.Quantity,
                        IsDeleted = false
                    };

                    context.DishIngredients.Add(ingredientToAdd);
                }
            }

            existingDish.UpdateNutritionFromIngredients();

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteDishAsync(int id)
        {
            using var context = CreateContext();

            var dish = await context.Dishes
                .Include(d => d.Ingredients)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (dish is null) return false;

            dish.IsDeleted = true;
            foreach (var ingredient in dish.Ingredients.Where(i => !i.IsDeleted))
                ingredient.IsDeleted = true;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreDishAsync(int id)
        {
            using var context = CreateContext();

            var dish = await context.Dishes
                .Include(d => d.Ingredients)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsDeleted);

            if (dish is null) return false;

            dish.IsDeleted = false;
            foreach (var ingredient in dish.Ingredients.Where(i => !i.IsDeleted))
                ingredient.IsDeleted = false;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CheckDishExistsAsync(int id)
        {
            using var context = CreateContext();

            return await context.Dishes.AnyAsync(d => d.Id == id && !d.IsDeleted);
        }
        #endregion

        #region Dish Ingredients
        public async Task<bool> AddIngredientToDishAsync(int dishId, int productId, double quantity)
        {
            using var context = CreateContext();

            var dish = await context.Dishes.FirstOrDefaultAsync(d => d.Id == dishId && !d.IsDeleted);
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == dishId && !p.IsDeleted);

            if (dish is null || product is null) return false;

            var dishIngredient = new DishIngredient()
            {
                DishId = dishId,
                ProductId = productId,
                Quantity = quantity,
                IsDeleted = false
            };

            context.DishIngredients.Add(dishIngredient);

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveIngredientFromDishAsync(int dishIngredientId)
        {
            using var context = CreateContext();

            var ingredient = await context.DishIngredients.FirstOrDefaultAsync(di => di.Id == dishIngredientId && !di.IsDeleted);

            if (ingredient is null) return false;

            ingredient.IsDeleted = true;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveIngredientFromAllDishesAsync(int dishIngredientId)
        {
            using var context = CreateContext();

            var ingredients = await context.DishIngredients.Where(di => di.Id == dishIngredientId && !di.IsDeleted).ToListAsync();

            if (ingredients is null) return false;

            foreach (var ingredient in ingredients) ingredient.IsDeleted = true;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreIngredientFromDishAsync(int dishIngredientId)
        {
            using var context = CreateContext();

            var ingredient = await context.DishIngredients.FirstOrDefaultAsync(di => di.Id == dishIngredientId && di.IsDeleted);

            if (ingredient is null) return false;

            ingredient.IsDeleted = false;

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreIngredientForAllDishesAsync(int dishIngredientId)
        {
            using var context = CreateContext();

            var ingredients = await context.DishIngredients.Where(di => di.Id == dishIngredientId && di.IsDeleted).ToListAsync();

            if (ingredients is null) return false;

            foreach (var ingredient in ingredients) ingredient.IsDeleted = false;

            return await context.SaveChangesAsync() > 0;
        }
        #endregion
    }
}

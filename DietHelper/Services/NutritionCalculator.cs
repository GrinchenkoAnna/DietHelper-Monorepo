using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public record IngredientCalculationDto(int UserProductId, double Quantity);

    public class NutritionCalculator
    {
        private readonly ApiService _apiService;

        public NutritionCalculator(ApiService apiService)
        {
            _apiService = apiService;
        }

        private static double CalculateNutritionValue(double value, double quantity)
        {
            return value * (quantity / 100);
        }

        //public NutritionInfo CalculateProductNutrition(Product product, double quantity)
        //{
        //    return product == null
        //        ? throw new ArgumentNullException(nameof(product))
        //        : new NutritionInfo
        //    {
        //        Calories = CalculateNutritionValue(product.NutritionFacts.Calories, quantity),
        //        Protein = CalculateNutritionValue(product.NutritionFacts.Protein, quantity),
        //        Fat = CalculateNutritionValue(product.NutritionFacts.Fat, quantity),
        //        Carbs = CalculateNutritionValue(product.NutritionFacts.Carbs, quantity)
        //    };
        //}

        //!!!
        public async Task<NutritionInfo> CalculateUserDishNutrition(IEnumerable<IngredientCalculationDto> ingredients)
        {
            var userDishNutrition = new NutritionInfo();

            foreach (var ingredient in ingredients)
            {
                var userProduct = await _apiService.GetUserProductsAsync(ingredient.UserProductId);
                var quantity = ingredient.Quantity;

                if (userProduct is not null)
                {
                    var userProductNutrition = userProduct.CustomNutrition;

                    userDishNutrition.Calories += CalculateNutritionValue(userProductNutrition.Calories, quantity);
                    userDishNutrition.Protein += CalculateNutritionValue(userProductNutrition.Protein, quantity);
                    userDishNutrition.Fat += CalculateNutritionValue(userProductNutrition.Fat, quantity);
                    userDishNutrition.Carbs += CalculateNutritionValue(userProductNutrition.Carbs, quantity);
                }
            }

            return userDishNutrition;
        }

        //убрать - это для старых классов
        //public NutritionInfo CalculateUserDishNutrition(IEnumerable<DishIngredient> ingredients)
        //{
        //    return ingredients == null
        //    ? throw new ArgumentNullException(nameof(ingredients))
        //    : ingredients.Aggregate(new NutritionInfo(), (total, item) =>
        //    {
        //        total.Calories += CalculateNutritionValue(item.Ingredient.NutritionFacts.Calories, item.Quantity);
        //        total.Protein += CalculateNutritionValue(item.Ingredient.NutritionFacts.Protein, item.Quantity);
        //        total.Fat += CalculateNutritionValue(item.Ingredient.NutritionFacts.Fat, item.Quantity);
        //        total.Carbs += CalculateNutritionValue(item.Ingredient.NutritionFacts.Carbs, item.Quantity);
        //        return total;
        //    });
        //}
    }
}

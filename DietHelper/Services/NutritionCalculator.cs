using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DietHelper.Services
{
    public class NutritionCalculator
    {
        private static double CalculateNutritionValue(double value, double quantity)
        {
            return value * (quantity / 100);
        }

        public NutritionInfo CalculateProductNutrition(Product product, double quantity)
        {
            return product == null
                ? throw new ArgumentNullException(nameof(product))
                : new NutritionInfo
            {
                Calories = CalculateNutritionValue(product.NutritionFacts.Calories, quantity),
                Protein = CalculateNutritionValue(product.NutritionFacts.Protein, quantity),
                Fat = CalculateNutritionValue(product.NutritionFacts.Fat, quantity),
                Carbs = CalculateNutritionValue(product.NutritionFacts.Carbs, quantity)
            };
        }

        public NutritionInfo CalculateDishNutrition(IEnumerable<DishIngredient> ingredients)
        {
            return ingredients == null
            ? throw new ArgumentNullException(nameof(ingredients))
            : ingredients.Aggregate(new NutritionInfo(), (total, item) =>
            {
                total.Calories += CalculateNutritionValue(item.Ingredient.NutritionFacts.Calories, item.Quantity);
                total.Protein += CalculateNutritionValue(item.Ingredient.NutritionFacts.Protein, item.Quantity);
                total.Fat += CalculateNutritionValue(item.Ingredient.NutritionFacts.Fat, item.Quantity);
                total.Carbs += CalculateNutritionValue(item.Ingredient.NutritionFacts.Carbs, item.Quantity);
                return total;
            });
        }
    }
}

using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;

namespace DietHelper.Common.Models.MealEntries
{
    public class UserMealEntryIngredient
    {
        public int Id { get; set; }
        public int UserMealEntryId { get; set; }
        public int UserProductId { get; set; }
        public decimal Quantity { get; set; }
        public string ProductNameSnapshot { get; set; } = string.Empty;
        public NutritionInfo ProductNutritionInfoSnapshot { get; set; } = new();
        public bool IsDeleted { get; set; }

        // навигация
        public UserMealEntry UserMealEntry { get; set; }
        public UserProduct UserProduct { get; set; }
    }
}

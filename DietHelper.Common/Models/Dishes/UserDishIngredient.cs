using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DietHelper.Common.Models.Dishes
{
    public class UserDishIngredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserDishId { get; set; }

        [Required]
        public int UserProductId { get; set; }
        public virtual UserProduct UserProduct { get; set; } = null!;

        [Required]
        public double Quantity { get; set; }

        public bool IsDeleted { get; set; } = false;

        public NutritionInfo CurrentNutrition { get; set; } = new();

        public void CalculateNutrition(UserProduct? userProduct)
        {
            if (UserProduct == null)
            {
                CurrentNutrition = new NutritionInfo();
                return;
            }

            var factor = Quantity / 100;
            var nutrition = UserProduct.CustomNutrition;

            CurrentNutrition = new NutritionInfo()
            {
                Calories = nutrition.Calories * factor,
                Protein = nutrition.Protein * factor,
                Fat = nutrition.Fat * factor,
                Carbs = nutrition.Carbs * factor
            };
        }
    }
}

using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DietHelper.Common.Models.Dishes
{
    public class DishIngredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DishId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public double Quantity { get; set; }

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("ProductId")]
        public virtual Product Ingredient { get; set; } = null!;

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; } = null!;

        [NotMapped]
        private double Factor => Quantity / 100;

        [NotMapped]
        public NutritionInfo CurrentNutrition
        {
            get
            {
                if (Ingredient == null) return new NutritionInfo();

                var factor = Quantity / 100;
                return new NutritionInfo()
                {
                    Calories = Ingredient.NutritionFacts.Calories * factor,
                    Protein = Ingredient.NutritionFacts.Protein * factor,
                    Fat = Ingredient.NutritionFacts.Fat * factor,
                    Carbs = Ingredient.NutritionFacts.Carbs * factor
                };
            }
        }
    }
}

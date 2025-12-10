using DietHelper.Common.Interfaces.Core;
using DietHelper.Common.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DietHelper.Common.Models.Dishes
{
    public class Dish : INutritional
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public Dish() {}

        [Required]
        public string Name { get; set; } = string.Empty;

        public NutritionInfo NutritionFacts { get; set; } = new();

        public bool IsDeleted { get; set; } = false;
        
        public virtual ICollection<DishIngredient> Ingredients { get; set; } = new List<DishIngredient>();

        [NotMapped]
        public NutritionInfo DishNutrition => new()
        {
            Calories = Ingredients.Sum(i => i.CurrentNutrition.Calories),
            Protein = Ingredients.Sum(i => i.CurrentNutrition.Protein),
            Fat = Ingredients.Sum(i => i.CurrentNutrition.Fat),
            Carbs = Ingredients.Sum(i => i.CurrentNutrition.Carbs)
        };

        public void UpdateNutritionFromIngredients()
        {
            NutritionFacts = DishNutrition;
        }
    }
}

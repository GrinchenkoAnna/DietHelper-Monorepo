using DietHelper.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Models.Dishes
{
    public class UserDish
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        //public virtual User User { get; set; } = null!;

        [Required]
        public string Name { get; set; } = string.Empty;

        public NutritionInfo NutritionFacts { get; set; } = new();

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<UserDishIngredient> Ingredients { get; set; } = new List<UserDishIngredient>();

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

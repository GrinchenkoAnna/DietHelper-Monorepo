using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Models.MealEntries
{
    public class UserMealEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? UserDishId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalQuantity { get; set; }
        public NutritionInfo TotalNutrition { get; set; } = new();
        public List<UserMealEntryIngredient> Ingredients { get; set; } = new();
        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; // служебная

        // навигация
        public User User { get; set; } 
        public UserDish UserDish { get; set; }
    }
}

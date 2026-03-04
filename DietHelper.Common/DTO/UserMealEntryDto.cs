using DietHelper.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.DTO
{
    public class UserMealEntryDto
    {
        public int Id { get; set; }
        public int? UserDishId { get; set; }
        public string? UserDishName { get; set; }
        public DateTime Date {  get; set; }
        public decimal TotalQuantity { get; set; }
        public NutritionInfo TotalNutrition { get; set; } = new();
        public List<UserMealEntryIngredientDto> Ingredients { get; set; } = new();
    }
}

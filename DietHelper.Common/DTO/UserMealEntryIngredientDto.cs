using DietHelper.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.DTO
{
    public class UserMealEntryIngredientDto
    {
        public int Id { get; set; }
        public int UserProductId { get; set; }
        public decimal Quantity { get; set; }
        public string ProductNameSnapshot { get; set; } = string.Empty;
        public NutritionInfo ProductNutritionInfoSnapshot { get; set; } = new();
    }
}

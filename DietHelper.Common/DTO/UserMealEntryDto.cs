using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.DTO
{
    public class UserMealEntryDto
    {
        public int UserDishId { get; set; }
        public DateTime Date {  get; set; }
        public List<UserMealEntryIngredientDto> Ingredients { get; set; } = new();
    }
}

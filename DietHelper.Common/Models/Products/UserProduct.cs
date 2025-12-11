using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Models.Products
{
    public class UserProduct
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? BaseProductId { get; set; }
        public virtual BaseProduct? BaseProduct { get; set; }

        public NutritionInfo CustomNutrition { get; set; } = new();

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<UserDishIngredient> DishIngredients { get; set; } = new List<UserDishIngredient>();
    }
}

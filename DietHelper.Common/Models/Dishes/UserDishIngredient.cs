using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Models.Dishes
{
    public class UserDishIngredient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserDishId { get; set; }
        public virtual UserDish UserDish { get; set; } = null!;

        [Required]
        public int UserProductId { get; set; }
        public virtual UserProduct UserProduct { get; set; } = null!;

        [Required]
        public double Quantity { get; set; }

        [NotMapped]
        private double Factor => Quantity / 100;

        public bool IsDeleted { get; set; } = false;        

        public NutritionInfo CurrentNutrition //нужно вообще??
        {
            get
            {
                if (UserProduct == null) return new NutritionInfo();

                var factor = Quantity / 100;
                var nutrition = UserProduct.CustomNutrition;

                return new NutritionInfo()
                {
                    Calories = nutrition.Calories * factor,
                    Protein = nutrition.Protein * factor,
                    Fat = nutrition.Fat * factor,
                    Carbs = nutrition.Carbs * factor
                };
            }
        }
    }
}

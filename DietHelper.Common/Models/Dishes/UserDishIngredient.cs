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
        public virtual UserDish Dish { get; set; } = null!;

        [Required]
        public int UserProductId { get; set; }
        public virtual UserProduct Product { get; set; } = null!;

        [Required]
        public double Quantity { get; set; }

        [NotMapped]
        private double Factor => Quantity / 100;

        public bool IsDeleted { get; set; } = false;        

        [NotMapped]
        public NutritionInfo CurrentNutrition
        {
            get
            {
                if (Product == null) return new NutritionInfo();

                var factor = Quantity / 100;
                var nutrition = Product.CustomNutrition;

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

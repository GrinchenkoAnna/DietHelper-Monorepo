using DietHelper.Common.Interfaces.Core;
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
    public class BaseProduct : INutritional
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public NutritionInfo NutritionFacts { get; set; } = new();

        public bool IsDeleted { get; set; } = false;

        //public virtual ICollection<UserProduct> UserProducts { get; set; } = new List<UserProduct>();
    }
}

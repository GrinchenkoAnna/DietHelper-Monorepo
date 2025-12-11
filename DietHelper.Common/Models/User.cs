using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Common.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? Name { get; set; }

        public virtual ICollection<UserProduct> Products { get; set; } = new List<UserProduct>();

        public virtual ICollection<UserDish> Dishes { get; set; } = new List<UserDish>();
    }
}

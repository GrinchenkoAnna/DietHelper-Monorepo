using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DietHelper.Common.Models.Core
{
    [Owned]
    public class NutritionInfo
    {
        [Column("Protein")]
        public double Protein { get; set; }

        [Column("Fat")]
        public double Fat { get; set; }

        [Column("Carbs")]
        public double Carbs { get; set; }

        [Column("Calories")]
        public double Calories { get; set; }

        public NutritionInfo() { }

        public NutritionInfo(double calories, double protein, double fat, double carbs)
        {
            Calories = calories;
            Protein = protein;
            Fat = fat;
            Carbs = carbs;            
        }

        public static NutritionInfo operator +(NutritionInfo a, NutritionInfo b) => new(a.Calories + b.Calories,
            a.Protein + b.Protein,
            a.Fat + b.Fat,
            a.Carbs + b.Carbs);

        public static NutritionInfo operator *(NutritionInfo a, double factor) => new(a.Calories * factor,
            a.Protein * factor,
            a.Fat * factor,
            a.Carbs * factor);

        [JsonIgnore]
        public string FormattedCalories => $"{Calories:F1} ккал";

        [JsonIgnore]
        public string FormattedProtein => $"{Protein:F1} г";

        [JsonIgnore]
        public string FormattedFat => $"{Fat:F1} г";

        [JsonIgnore]
        public string FormattedCarbs => $"{Carbs:F1} г";
    }
}

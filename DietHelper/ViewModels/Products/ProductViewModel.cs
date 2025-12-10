using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;

using System;
using System.ComponentModel.DataAnnotations;

namespace DietHelper.ViewModels.Products
{
    public partial class ProductViewModel : ObservableValidator
    {
        [ObservableProperty]
        private int id = -1;

        [ObservableProperty]
        [Required(ErrorMessage = "Название обязательно")]
        [MinLength(1, ErrorMessage = "Название не может быть пустым")]
        private string? name;

        [ObservableProperty]
        [Range(0, double.MaxValue, ErrorMessage = "Калории не могут быть отрицательными")]
        private double calories;

        [ObservableProperty]
        [Range(0, double.MaxValue, ErrorMessage = "Белки не могут быть отрицательными")]
        private double protein;

        [ObservableProperty]
        [Range(0, double.MaxValue, ErrorMessage = "Жиры не могут быть отрицательными")]
        private double fat;

        [ObservableProperty]
        [Range(0, double.MaxValue, ErrorMessage = "Углеводы не могут быть отрицательными")]
        private double carbs;

        [ObservableProperty]
        private double quantity = 100;

        [ObservableProperty]
        private NutritionInfo totalNutritionInfo = new();
        private Product newIngredient;

        public ProductViewModel()
        {
            Recalculate();
        }

        public ProductViewModel(Product product)
        {
            Id = product.Id;
            Name = product.Name;
            Calories = product.NutritionFacts?.Calories ?? 0;
            Protein = product.NutritionFacts?.Protein ?? 0;
            Fat = product.NutritionFacts?.Fat ?? 0;
            Carbs = product.NutritionFacts?.Carbs ?? 0;

            Recalculate();
        }

        public NutritionInfo BaseNutrition => new()
        {
            Calories = Calories,
            Protein = Protein,
            Fat = Fat,
            Carbs = Carbs
        };

        public event EventHandler ProductChanged;

        protected void Recalculate()
        {
            var factor = Quantity / 100.0;

            TotalNutritionInfo = new()
            {
                Calories = Calories * factor,
                Protein = Protein * factor,
                Fat = Fat * factor,
                Carbs = Carbs * factor
            };

            ValidateAllProperties();
            ProductChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnQuantityChanged(double value) => Recalculate();

        public Product ToModel()
        {
            return new Product()
            {
                Id = Id,
                Name = Name ?? string.Empty,
                NutritionFacts = BaseNutrition
            };
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;

using System;
using System.ComponentModel.DataAnnotations;

namespace DietHelper.ViewModels.Products
{
    public partial class UserProductViewModel : ObservableValidator
    {
        [ObservableProperty]
        private int id = -1;

        [ObservableProperty]
        private int userId = -1;

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
        private NutritionInfo nutritionFacts = new();

        public UserProductViewModel()
        {
            Recalculate();
        }

        public UserProductViewModel(UserProduct userProduct)
        {
            Id = userProduct.Id;
            UserId = userProduct.UserId;
            Name = userProduct.BaseProduct?.Name;
            Calories = userProduct.CustomNutrition?.Calories 
                ?? userProduct.BaseProduct?.NutritionFacts?.Calories 
                ?? 0;
            Protein = userProduct.CustomNutrition?.Protein
                ?? userProduct.BaseProduct?.NutritionFacts?.Protein 
                ?? 0;
            Fat = userProduct.CustomNutrition?.Fat
                ?? userProduct.BaseProduct?.NutritionFacts?.Fat 
                ?? 0;
            Carbs = userProduct.CustomNutrition?.Carbs
                ?? userProduct.BaseProduct?.NutritionFacts?.Carbs 
                ?? 0;

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

            NutritionFacts = new()
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
    }
}

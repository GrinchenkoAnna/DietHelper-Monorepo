using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Base
{
    public abstract partial class ProductViewModelBase : ObservableValidator
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
        private NutritionInfo nutritionFacts = new();

        protected bool isManualQuantity = false;

        public ProductViewModelBase()
        {
            ProductChanged += (sender, e) => { };
        }

        public event EventHandler ProductChanged;

        protected void Recalculate()
        {
            if (isManualQuantity) return;

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

using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.ViewModels.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace DietHelper.ViewModels.Products
{
    public partial class UserProductViewModel : ProductViewModelBase
    {
        [ObservableProperty]
        private int userId = -1;

        [ObservableProperty]
        private int mealEntryId;

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
    }
}

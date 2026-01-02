using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.ViewModels.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace DietHelper.ViewModels.Products
{
    public partial class BaseProductViewModel : ProductViewModelBase
    {
        public BaseProductViewModel()
        {
            Recalculate();
        }

        public BaseProductViewModel(BaseProduct baseProduct)
        {
            Id = baseProduct.Id;
            Name = baseProduct.Name;
            Calories = baseProduct.NutritionFacts?.Calories ?? 0;
            Protein = baseProduct.NutritionFacts?.Protein ?? 0;
            Fat = baseProduct.NutritionFacts?.Fat ?? 0;
            Carbs = baseProduct.NutritionFacts?.Carbs ?? 0;

            Recalculate();
        }

        public BaseProduct ToModel()
        {
            return new BaseProduct()
            {
                Id = Id,
                Name = Name ?? string.Empty,
                NutritionFacts = NutritionFacts,
                IsDeleted = false
            };
        }
    }
}

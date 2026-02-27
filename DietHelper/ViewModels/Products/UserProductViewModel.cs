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

        // загрузка из справочника
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

        // загрузка из истории
        public UserProductViewModel(int id, string name, double quantity, NutritionInfo totalNutrition)
        {
            Id = id;
            Name = name;

            if (quantity != 100)
            {
                isManualQuantity = true;
                RestoreNutritionForUserProduct(totalNutrition, quantity);
                NutritionFacts = totalNutrition;
                Quantity = quantity;
                isManualQuantity = false;
            }
            else
            {
                Calories = totalNutrition.Calories;
                Protein = totalNutrition.Protein;
                Fat = totalNutrition.Fat;
                Carbs = totalNutrition.Carbs;
                NutritionFacts = totalNutrition;
                Quantity = quantity;
            }
        }

        private void RestoreNutritionForUserProduct(NutritionInfo totalNutrition, double quantity)
        {
            Calories = totalNutrition.Calories * 100 / quantity;
            Protein = totalNutrition.Protein * 100 / quantity;
            Fat = totalNutrition.Fat * 100 / quantity;
            Carbs = totalNutrition.Carbs * 100 / quantity;
        }
    }
}

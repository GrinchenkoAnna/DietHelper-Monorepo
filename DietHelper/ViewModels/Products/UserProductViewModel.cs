using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.ViewModels.Base;
using System.ComponentModel;

namespace DietHelper.ViewModels.Products
{
    public partial class UserProductViewModel : ProductViewModelBase
    {
        [ObservableProperty]
        private int userId = -1;

        [ObservableProperty]
        private int mealEntryId;

        [ObservableProperty]
        private bool isInAddMode;

        public bool ShowDirtyIndicator => !IsInAddMode && IsDirty;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(IsDirty) || e.PropertyName == nameof(IsInAddMode))
                OnPropertyChanged(nameof(ShowDirtyIndicator));
        }

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
            IsDirty = false;
            isInAddMode = true;
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

            IsDirty = false;
            IsInAddMode = false;
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

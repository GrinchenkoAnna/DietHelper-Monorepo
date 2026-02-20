using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class UserDishIngredientViewModel : ObservableValidator
    {
        [ObservableProperty]
        private int id = -1;

        [ObservableProperty]
        private int userDishId = -1;

        [ObservableProperty]
        private int userProductId = -1;

        [ObservableProperty]
        [Required(ErrorMessage = "Название обязательно")]
        [MinLength(1, ErrorMessage = "Название не может быть пустым")]
        private string? name;

        [ObservableProperty]
        [Range(0.1, double.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
        private double quantity = 100;

        [ObservableProperty]
        private NutritionInfo currentNutrition = new();

        [ObservableProperty]
        private string productNameSnapshot = string.Empty;

        [ObservableProperty]
        private NutritionInfo productNutritionInfoSnapshot = new();

        public UserDishIngredientViewModel() {}

        public UserDishIngredientViewModel(UserDishIngredient userDishIngredient)
        {
            Id = userDishIngredient.Id;
            UserDishId = userDishIngredient.UserDishId;
            UserProductId = userDishIngredient.UserProductId;
            Name = userDishIngredient.UserProduct.BaseProduct?.Name;
            Quantity = userDishIngredient.Quantity;
            CurrentNutrition = userDishIngredient.CurrentNutrition;

            if (userDishIngredient.UserProduct is not null)
            {
                ProductNameSnapshot = userDishIngredient.UserProduct.BaseProduct!.Name;
                ProductNutritionInfoSnapshot = userDishIngredient.UserProduct.CustomNutrition ?? userDishIngredient.UserProduct.BaseProduct.NutritionFacts;
            }
        }

        public UserDishIngredient ToModel()
        {
            return new UserDishIngredient
            {
                //Id должен генерироваться на сервере
                UserDishId = UserDishId,
                UserProductId = UserProductId,
                Quantity = Quantity,
                //IsDeleted = false
            };
        }
    }
}

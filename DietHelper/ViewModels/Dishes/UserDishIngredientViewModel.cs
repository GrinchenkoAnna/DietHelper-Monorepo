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
        private double quantity = 100;

        [ObservableProperty]
        private NutritionInfo currentNutrition = new();

        public UserDishIngredientViewModel() {}

        public UserDishIngredientViewModel(UserDishIngredient userDishIngredient)
        {
            Id = userDishIngredient.Id;
            UserDishId = userDishIngredient.UserDishId;
            UserProductId = userDishIngredient.UserProductId;
            Name = userDishIngredient.UserProduct.BaseProduct?.Name;
            Quantity = userDishIngredient.Quantity;
            CurrentNutrition = userDishIngredient.CurrentNutrition;
        }

        public UserDishIngredient ToModel()
        {
            return new UserDishIngredient
            {
                Id = Id,
                UserDishId = UserDishId,
                UserProductId = UserProductId,
                Quantity = Quantity,
                IsDeleted = false
            };
        }
    }
}

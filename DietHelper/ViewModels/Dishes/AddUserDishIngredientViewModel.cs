using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using DietHelper.ViewModels.Products;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class AddUserDishIngredientViewModel : AddItemBaseViewModel<UserProduct, UserProductViewModel>
    {
        public AddUserDishIngredientViewModel(ApiService _apiService) : base(_apiService) { }

        [ObservableProperty] private BaseProductViewModel? selectedBaseItem;

        [ObservableProperty] private double quantity = 100;

        public ObservableCollection<BaseProductViewModel> BaseSearchResults { get; } = new();

        public ObservableCollection<BaseProductViewModel> AllBaseItems { get; } = new();

        protected override async void InitializeData()
        {
            var userProducts = await _apiService.GetUserProductsAsync();

            foreach (var userProduct in userProducts)
            {
                if (userProduct.Id > 0)
                {
                    UserSearchResults.Add(new UserProductViewModel(userProduct));
                    AllUserItems.Add(new UserProductViewModel(userProduct));
                }
            }

            var baseProducts = await _apiService.GetBaseProductsAsync();

            foreach (var baseProduct in baseProducts)
            {
                if (baseProduct.Id > 0)
                {
                    BaseSearchResults.Add(new BaseProductViewModel(baseProduct));
                    AllBaseItems.Add(new BaseProductViewModel(baseProduct));
                }
            }
        }

        protected override async Task<UserProduct> CreateNewUserItem()
        {
            return await CreateProductAsync();
        }

        protected override void AddUserItem()
        {
            if (SelectedUserItem is null) return;

            var ingredient = new UserDishIngredientViewModel
            {
                UserProductId = SelectedUserItem.Id,
                Name = SelectedUserItem.Name,
                Quantity = SelectedUserItem.Quantity,
                CurrentNutrition = new NutritionInfo()
                {
                    Calories = SelectedUserItem.NutritionFacts.Calories,
                    Protein = SelectedUserItem.NutritionFacts.Protein,
                    Fat = SelectedUserItem.NutritionFacts.Fat,
                    Carbs = SelectedUserItem.NutritionFacts.Carbs
                }
            };

            WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(ingredient));
        }

        private async void AddBaseItem()
        {
            if (SelectedBaseItem is null) return;

            User user = await GetCurrentUser();

            var userProduct = new UserProduct()
            {
                UserId = GetCurrentUserId(),
                User = user,
                BaseProductId = SelectedBaseItem.Id,
                BaseProduct = SelectedBaseItem.ToModel(),
                CustomNutrition = new NutritionInfo()
                {
                    Calories = ManualCalories,
                    Protein = ManualProtein,
                    Fat = ManualFat,
                    Carbs = ManualCarbs
                },
                IsDeleted = false
            };

            WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(new UserProductViewModel(userProduct)));
        }

        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newProduct = CreateNewUserItem();

            var userIngredient = new UserDishIngredientViewModel
            {
                UserProductId = newProduct.Id,
                Name = ManualName!,
                Quantity = Quantity,
                CurrentNutrition = new NutritionInfo()
                {
                    Calories = ManualCalories * (Quantity / 100),
                    Protein = ManualProtein * (Quantity / 100),
                    Fat = ManualFat * (Quantity / 100),
                    Carbs = ManualCarbs * (Quantity / 100)
                }
            };

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(userIngredient));
        }

        protected override async void DeleteItemFromDatabase(UserProductViewModel userProductViewModel)
        {
            RemoveFromCollections(userProductViewModel)

            await _apiService.DeleteUserProductAsync(userProductViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserProductMessage(userProductViewModel.Id));
        }
    }
}

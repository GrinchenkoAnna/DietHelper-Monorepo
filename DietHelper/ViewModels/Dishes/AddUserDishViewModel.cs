using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class AddUserDishViewModel : AddItemBaseViewModel<UserDish, UserDishViewModel>
    {
        private readonly NutritionCalculator _nutritionCalculator;

        public AddUserDishViewModel(ApiService apiService, NutritionCalculator nutritionCalculator) : base(apiService)
        {
            _nutritionCalculator = nutritionCalculator;
        }

        protected override async void InitializeData()
        {
            var dishes = await _apiService.GetUserDishesAsync();

            foreach (var dish in dishes)
            {
                if (dish.Id > 0)
                {
                    UserSearchResults.Add(new UserDishViewModel(dish, _nutritionCalculator, _apiService));
                    AllUserItems.Add(new UserDishViewModel(dish, _nutritionCalculator, _apiService));
                }
            }
        }

        protected override void AddUserItem()
        {
            if (SelectedUserItem is not null)
            {
                WeakReferenceMessenger.Default.Send(new AddUserDishClosedMessage(SelectedUserItem));
            }
        }

        protected override async Task<UserDish> CreateNewUserItem()
        {
            User user = await GetCurrentUser();

            var userDish = new UserDish()
            {
                UserId = user.Id,
                Name = ManualName!,
                NutritionFacts = new NutritionInfo()
                {
                    Calories = ManualCalories,
                    Protein = ManualProtein,
                    Fat = ManualFat,
                    Carbs = ManualCarbs
                },
                IsDeleted = false
            };

            return await _apiService.AddUserDishAsync(userDish);
        }

        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newUserDish = await CreateNewUserItem();

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddUserDishClosedMessage(new UserDishViewModel(newUserDish, _nutritionCalculator, _apiService)));
        }

        protected override async void DeleteItemFromDatabase(UserDishViewModel userDishViewModel)
        {
            UserSearchResults.Remove(userDishViewModel);
            AllUserItems.Remove(userDishViewModel);
            await _apiService.DeleteDishAsync(userDishViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserDishMessage(userDishViewModel.Id));
        }
    }
}

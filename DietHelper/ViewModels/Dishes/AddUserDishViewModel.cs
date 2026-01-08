using CommunityToolkit.Mvvm.ComponentModel;
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
    public enum DishType
    {
        EmptyDish,
        ReadyDish
    }

    public partial class AddUserDishViewModel : AddItemBaseViewModel<UserDish, UserDishViewModel>
    {
        private readonly NutritionCalculator _nutritionCalculator;

        [ObservableProperty]
        private DishType selectedDishType = DishType.EmptyDish;

        [ObservableProperty]
        private bool isNutritionEnabled = false;

        public bool IsEmptyDish
        {
            get => SelectedDishType == DishType.EmptyDish;
            set
            {
                if (value) SelectedDishType = DishType.EmptyDish;
            }
        }

        public bool IsReadyDish
        {
            get => SelectedDishType == DishType.ReadyDish;
            set
            {
                if (value) SelectedDishType = DishType.ReadyDish;
            }
        }

        public AddUserDishViewModel(ApiService apiService, NutritionCalculator nutritionCalculator) : base(apiService)
        {
            _nutritionCalculator = nutritionCalculator;

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedDishType))
                {
                    OnPropertyChanged(nameof(IsEmptyDish));
                    OnPropertyChanged(nameof(IsReadyDish));
                    UpdateNutritionState();
                }                    
            };

        }

        private void UpdateNutritionState()
        {
            IsNutritionEnabled = (SelectedDishType == DishType.ReadyDish);

            if (IsNutritionEnabled)
            {
                ManualCalories = 0;
                ManualProtein = 0;
                ManualFat = 0;
                ManualCarbs = 0;
            }
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
                IsReadyDish = SelectedDishType == DishType.ReadyDish,
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
            RemoveFromCollections(userDishViewModel);

            await _apiService.DeleteDishAsync(userDishViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserDishMessage(userDishViewModel.Id));
        }


    }
}

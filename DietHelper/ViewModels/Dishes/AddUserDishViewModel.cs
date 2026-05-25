using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using DietHelper.ViewModels.Products;
using System;
using System.Collections.Generic;
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

        public AddUserDishViewModel(IApiService _apiService, INotificationService _notificationService) : base(_apiService, _notificationService)
        {

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

            if (dishes is not null)
            {
                foreach (var dish in dishes)
                {
                    if (dish.Id > 0)
                    {
                        UserSearchResults.Add(new UserDishViewModel(dish, _apiService));
                        AllUserItems.Add(new UserDishViewModel(dish, _apiService));
                    }
                }
            }
            else
                _notificationService.ShowInfo("Блюда не загружены", "Возможна ошибка сети или сервера. Проверьте подключение и попробуйте снова");
        }

        protected override async Task DoSearch(string? term)
        {
            IsBusy = true;

            var userResults = new List<UserDishViewModel>();

            await Task.Run(() =>
            {
                bool isTermEmpty = string.IsNullOrWhiteSpace(term);

                foreach (var item in AllUserItems)
                {
                    if (isTermEmpty || (item.Name ?? "").Contains(term!, StringComparison.CurrentCultureIgnoreCase))
                        userResults.Add(item);
                }
            });

            UserSearchResults.Clear();
            foreach (var item in userResults) UserSearchResults.Add(item);

            IsBusy = false;
        }

        protected override void AddUserItem()
        {
            if (SelectedUserItem is not null)
                WeakReferenceMessenger.Default.Send(new AddUserDishClosedMessage(SelectedUserItem));
            else
                _notificationService.ShowError("Не удалось создать блюдо", "Попробуйте выбрать блюдо снова");
        }

        protected override async Task<UserDish?> CreateNewUserItem()
        {
            var userDish = new UserDish()
            {
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
            if (string.IsNullOrEmpty(ManualName))
            {
                _notificationService.ShowError("Создание блюда", "Имя блюда не должно быть пустым");
                return;
            }

            var newUserDish = await CreateNewUserItem();

            if (newUserDish is not null)
            {
                ClearManualEntries();

                WeakReferenceMessenger.Default.Send(new AddUserDishClosedMessage(new UserDishViewModel(newUserDish, _apiService)));
            }
            else
            {
                _notificationService.ShowError("Ошибка добавления блюда", "Возможна ошибка сети или сервера. Проверьте подключение и попробуйте снова");
            }
        }

        protected override async void DeleteItemFromDatabase(UserDishViewModel userDishViewModel)
        {
            RemoveFromCollections(userDishViewModel);

            await _apiService.DeleteDishAsync(userDishViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserDishMessage(userDishViewModel.Id));
        }
    }
}

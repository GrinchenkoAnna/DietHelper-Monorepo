using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.Core;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Dishes;
using DietHelper.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        private DateTime _currentWeekStart;

        public ObservableCollection<DateTime> WeekDays { get; } = new();

        private ObservableCollection<UserProductViewModel> _userProducts = [];

        private static DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void UpdateWeekDays()
        {
            WeekDays.Clear();
            for (int i = 0; i < 7; i++)
                WeekDays.Add(_currentWeekStart.AddDays(i));
        }

        partial void OnSelectedDateChanged(DateTime value)
        {
            if (value < _currentWeekStart || value >= _currentWeekStart.AddDays(7))
            {
                _currentWeekStart = GetStartOfWeek(value);
                UpdateWeekDays();
            }
            _ = LoadDataForDateAsync(value);
        }

        [RelayCommand]
        private void SelectPreviousWeek()
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            UpdateWeekDays();
        }

        [RelayCommand]
        private void SelectNextWeek()
        {
            _currentWeekStart = _currentWeekStart.AddDays(+7);
            UpdateWeekDays();
        }

        [RelayCommand]
        private void SetSelectedDate(DateTime date)
        {
            SelectedDate = date;
        }

        public ObservableCollection<UserProductViewModel> UserProducts
        {
            get => _userProducts;
            set
            {
                if (_userProducts is not null)
                {
                    _userProducts.CollectionChanged -= OnUserProductsCollectionChanged;
                    UnsubscribeFromUserProducts(_userProducts);
                }

                SetProperty(ref _userProducts, value);

                if (_userProducts is not null)
                {
                    _userProducts.CollectionChanged += OnUserProductsCollectionChanged;
                    SubscribeToUserProducts(_userProducts);
                }

                UpdateTotals();
            }
        }

        private ObservableCollection<UserDishViewModel> _userDishes = [];

        public ObservableCollection<UserDishViewModel> UserDishes
        {
            get => _userDishes;
            set
            {
                if (_userDishes is not null)
                {
                    _userDishes.CollectionChanged -= OnUserDishesCollectionChanged;
                    UnsubscribeFromUserDishes(_userDishes);
                }

                SetProperty(ref _userDishes, value);

                if (_userDishes is not null)
                {
                    _userDishes.CollectionChanged += OnUserDishesCollectionChanged;
                    SubscribeToUserDishes(_userDishes);
                }

                UpdateTotals();
            }
        }

        private void SubscribeToUserProducts(IEnumerable<UserProductViewModel> userProducts)
        {
            foreach (var userProduct in userProducts)
                userProduct.PropertyChanged += OnUserProductPropertyChanged;
        }
        private void UnsubscribeFromUserProducts(IEnumerable<UserProductViewModel> userProducts)
        {
            foreach (var userProduct in userProducts)
                userProduct.PropertyChanged -= OnUserProductPropertyChanged;
        }
        private void OnUserProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserProductViewModel.Quantity) ||
                e.PropertyName == nameof(UserProductViewModel.NutritionFacts))
                UpdateTotals();
        }

        private void SubscribeToUserDishes(IEnumerable<UserDishViewModel> userDishes)
        {
            foreach (var userDish in userDishes)
                userDish.PropertyChanged += OnUserDishPropertyChanged;
        }
        private void UnsubscribeFromUserDishes(IEnumerable<UserDishViewModel> userDishes)
        {
            foreach (var userDish in userDishes)
                userDish.PropertyChanged -= OnUserDishPropertyChanged;
        }
        private void OnUserDishPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserDishViewModel.Quantity) ||
                e.PropertyName == nameof(UserDishViewModel.NutritionFacts))
                UpdateTotals();
        }

        private void OnUserProductsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
                SubscribeToUserProducts(e.NewItems.Cast<UserProductViewModel>());

            if (e.OldItems is not null)
                UnsubscribeFromUserProducts(e.OldItems.Cast<UserProductViewModel>());

            UpdateTotals();
        }
        private void OnUserDishesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
                SubscribeToUserDishes(e.NewItems.Cast<UserDishViewModel>());

            if (e.OldItems is not null)
                UnsubscribeFromUserDishes(e.OldItems.Cast<UserDishViewModel>());

            UpdateTotals();
        }

        private async Task LoadDataForDateAsync(DateTime date)
        {
            try
            {
                var userMealEntriesDto = await _apiService.GetUserMealsForDate(date);
                if (userMealEntriesDto is null) return;

                UserProducts.Clear();
                UserDishes.Clear();

                foreach (var userMealEntryDto in userMealEntriesDto)
                {
                    // отдельный продукт
                    if (!userMealEntryDto.UserDishId.HasValue)
                    {
                        var product = userMealEntryDto.Ingredients.FirstOrDefault();
                        if (product is null) continue;

                        var productNutritionInfo = product.ProductNutritionInfoSnapshot;

                        var userProduct = new UserProductViewModel()
                        {
                            Id = product.UserProductId,
                            MealEntryId = product.Id,
                            Name = product.ProductNameSnapshot,
                            Calories = productNutritionInfo.Calories,
                            Protein = productNutritionInfo.Protein,
                            Fat = productNutritionInfo.Fat,
                            Carbs = productNutritionInfo.Carbs,
                            Quantity = (double)product.Quantity
                        };

                        UserProducts.Add(userProduct);
                    }
                    else // блюдо
                    {
                        var userDish = await _apiService.GetUserDishAsync(userMealEntryDto.UserDishId.Value);
                        if (userDish is null) continue;

                        var userDishViewModel = new UserDishViewModel(userDish, new NutritionCalculator(_apiService), _apiService)
                        {
                            MealEntryId = userMealEntryDto.Id,
                            Quantity = (double)userMealEntryDto.TotalQuantity,
                            NutritionFacts = userMealEntryDto.TotalNutrition,
                            IsReadyDish = (userMealEntryDto.Ingredients.Count == 0)
                        };

                        if (!userDishViewModel.IsReadyDish)
                        {
                            // нужно строить ингредиенты заново, потому что эта версия блюда зафиксирована и, скорее всего, не совпадает с текущей (динамичной)
                            foreach (var ingredient in userMealEntryDto.Ingredients)
                            {
                                var dishIngredient = new UserDishIngredientViewModel()
                                {
                                    Id = ingredient.Id,
                                    UserProductId = ingredient.UserProductId,
                                    UserDishId = userDishViewModel.Id,
                                    Name = ingredient.ProductNameSnapshot,
                                    CurrentNutrition = ingredient.ProductNutritionInfoSnapshot,
                                    ProductNameSnapshot = ingredient.ProductNameSnapshot,
                                    ProductNutritionInfoSnapshot = ingredient.ProductNutritionInfoSnapshot,
                                };
                                userDishViewModel.Ingredients.Add(dishIngredient);
                            }

                            UserDishes.Add(userDishViewModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                await LoadDataForDateAsync(SelectedDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }


        public MainWindowViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            InitializeAsync();

            _currentWeekStart = GetStartOfWeek(SelectedDate);
            UpdateWeekDays();

            int products = UserProducts.Count;
            int Dishes = UserDishes.Count;

            _userProducts.CollectionChanged += OnUserProductsCollectionChanged;
            _userDishes.CollectionChanged += OnUserDishesCollectionChanged;

            SubscribeToUserProducts(_userProducts);
            SubscribeToUserDishes(_userDishes);

            UpdateTotals();
        }

        [ObservableProperty]
        private NutritionInfo totalNutrition = new();

        [ObservableProperty]
        private double totalQuantity = 0;
        [ObservableProperty]
        private string formattedTotalQuantity;

        private void UpdateTotals()
        {
            UpdateTotalNutrition();
            UpdateTotalQuantity();
        }

        private void UpdateTotalNutrition()
        {
            var totalProductCalories = UserProducts.Sum(up => up.NutritionFacts.Calories);
            var totalProductProtein = UserProducts.Sum(up => up.NutritionFacts.Protein);
            var totalProductFat = UserProducts.Sum(up => up.NutritionFacts.Fat);
            var totalProductCarbs = UserProducts.Sum(up => up.NutritionFacts.Carbs);

            var totalDishCalories = UserDishes.Sum(ud => ud.NutritionFacts.Calories);
            var totalDishProtein = UserDishes.Sum(ud => ud.NutritionFacts.Protein);
            var totalDishFat = UserDishes.Sum(ud => ud.NutritionFacts.Fat);
            var totalDishCarbs = UserDishes.Sum(ud => ud.NutritionFacts.Carbs);

            TotalNutrition = new NutritionInfo
            {
                Calories = totalProductCalories + totalDishCalories,
                Protein = totalProductProtein + totalDishProtein,
                Fat = totalProductFat + totalDishFat,
                Carbs = totalProductCarbs + totalDishCarbs
            };
        }

        private void UpdateTotalQuantity()
        {
            var userProductsQuantity = UserProducts.Sum(up => up.Quantity);
            var userDishesQuantity = UserDishes.Sum(d => d.Quantity);

            TotalQuantity = userProductsQuantity + userDishesQuantity;
            FormattedTotalQuantity = $"{TotalQuantity} г";
        }

        [RelayCommand]
        private async Task AddUserProduct()
        {
            try
            {
                var userProduct = await WeakReferenceMessenger.Default.Send(new AddUserProductMessage());
                if (userProduct is null) return;

                var userMealEntryDto = new UserMealEntryDto()
                {
                    Date = SelectedDate,
                    Ingredients = new()
                    {
                        new UserMealEntryIngredientDto()
                        {
                            UserProductId = userProduct.Id,
                            Quantity = (decimal)userProduct.Quantity,
                            ProductNameSnapshot = userProduct.Name!,
                            ProductNutritionInfoSnapshot = userProduct.NutritionFacts
                        }
                    }
                };

                var userMealEntry = _apiService.AddUserMealEntryAsync(userMealEntryDto);

                if (userMealEntry is not null)
                    await LoadDataForDateAsync(SelectedDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RemoveUserProduct(UserProductViewModel userProduct)
        {
            if (await _apiService.DeleteUserMealEntryAsync(userProduct.MealEntryId))
                UserProducts.Remove(userProduct);
        }

        [RelayCommand]
        private async Task AddDish()
        {
            try
            {
                var userDish = await WeakReferenceMessenger.Default.Send(new AddUserDishMessage());
                if (userDish is null) return;

                var ingredients = userDish.Ingredients.Select(ingredient => new UserMealEntryIngredientDto()
                {
                    UserProductId = ingredient.UserProductId,
                    Quantity = (decimal)ingredient.Quantity,
                    ProductNameSnapshot = ingredient.ProductNameSnapshot,
                    ProductNutritionInfoSnapshot = ingredient.ProductNutritionInfoSnapshot
                }).ToList();
                if (ingredients is null || ingredients.Count == 0) return;

                var userMealEntryDto = new UserMealEntryDto()
                {
                    UserDishId = userDish.Id,
                    Date = SelectedDate,
                    Ingredients = ingredients
                };

                var userMealEntry = _apiService.AddUserMealEntryAsync(userMealEntryDto);

                if (userMealEntry is not null)
                    await LoadDataForDateAsync(SelectedDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RemoveDish(UserDishViewModel userDish)
        {
            if (await _apiService.DeleteUserMealEntryAsync(userDish.MealEntryId))
                UserDishes.Remove(userDish);
        }

        [RelayCommand]
        private async Task SaveDay()
        {
            try
            {
                foreach (var userProduct in UserProducts)
                {
                    if (userProduct.MealEntryId > 0)
                    {
                        var userMealEntryProduct = new UserMealEntryDto()
                        {
                            Date = SelectedDate,
                            Ingredients = new()
                            {
                                new UserMealEntryIngredientDto()
                                {
                                    UserProductId = userProduct.Id,
                                    Quantity = (decimal)userProduct.Quantity,
                                    ProductNameSnapshot = userProduct.Name!,
                                    ProductNutritionInfoSnapshot = userProduct.NutritionFacts
                                }
                            }
                        };

                        await _apiService.UpdateUserMealEntry(userProduct.MealEntryId, userMealEntryProduct);
                    }
                }

                foreach (var userDish in UserDishes)
                {
                    if (userDish.MealEntryId > 0)
                    {
                        var ingredients = new List<UserMealEntryIngredientDto>();

                        foreach (var ingredient in userDish.Ingredients)
                        {
                            ingredients.Add(new UserMealEntryIngredientDto()
                            {
                                UserProductId = ingredient.UserProductId,
                                Quantity = (decimal)ingredient.Quantity,
                                ProductNameSnapshot = ingredient.ProductNameSnapshot,
                                ProductNutritionInfoSnapshot = ingredient.ProductNutritionInfoSnapshot
                            });
                        }

                        var userMealEntryDish = new UserMealEntryDto()
                        {
                            UserDishId = userDish.Id,
                            Date = SelectedDate,
                            Ingredients = ingredients
                        };

                        await _apiService.UpdateUserMealEntry(userDish.MealEntryId, userMealEntryDish);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }


        [RelayCommand]
        private void Logout()
        {
            try
            {
                _apiService.Logout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowViewModel]: {ex.Message}");
            }
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.MealEntries;
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
        private readonly IApiService _apiService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        private DateTime _currentWeekStart;
        public ObservableCollection<DateTime> WeekDays { get; } = new();

        private ObservableCollection<object> _allEntries = new();
        public ObservableCollection<object> AllEntries
        {
            get => _allEntries;
            set => SetProperty(ref _allEntries, value);
        }
        public ObservableCollection<object> BreakfastEntries { get; } = new();
        public ObservableCollection<object> LunchEntries { get; } = new();
        public ObservableCollection<object> DinnerEntries { get; } = new();
        public ObservableCollection<object> SnackEntries { get; } = new();

        // обратная совместимость, временно
        public IEnumerable<UserProductViewModel> UserProducts => AllEntries.OfType<UserProductViewModel>();
        public IEnumerable<UserDishViewModel> UserDishes => AllEntries.OfType<UserDishViewModel>();

        [ObservableProperty]
        private string entriesMessage = string.Empty;
        public bool HasEntries => AllEntries.Count > 0;

        [ObservableProperty]
        private NutritionInfo totalNutrition = new();

        [ObservableProperty]
        private double totalQuantity = 0;
        [ObservableProperty]
        private string formattedTotalQuantity;


        #region Subscribes
        private void SubscribeToEntries(object entry)
        {
            if (entry is INotifyPropertyChanged notifiable)
                notifiable.PropertyChanged += OnEntryPropertyChanged;
        }
        private void UnsubscribeFromEntries(object entry)
        {
            if (entry is INotifyPropertyChanged notifiable)
                notifiable.PropertyChanged -= OnEntryPropertyChanged;
        }
        private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserProductViewModel.Quantity) ||
                e.PropertyName == nameof(UserProductViewModel.NutritionFacts) ||
                e.PropertyName == nameof(UserDishViewModel.Quantity) ||
                e.PropertyName == nameof(UserDishViewModel.NutritionFacts))
                UpdateTotals();
        }

        private void OnAllEntriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
                foreach (var item in e.NewItems)
                    SubscribeToEntries(item);

            if (e.OldItems is not null)
                foreach (var item in e.OldItems)
                    UnsubscribeFromEntries(item);

            OnPropertyChanged(nameof(UserProducts));
            OnPropertyChanged(nameof(UserDishes));

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            UpdateTotalNutrition();
            UpdateTotalQuantity();
        }

        private void UpdateTotalNutrition()
        {
            double calories = 0, protein = 0, fat = 0, carbs = 0;

            foreach (var entry in AllEntries)
            {
                if (entry is UserProductViewModel userProduct)
                {
                    calories += userProduct.Calories;
                    protein += userProduct.Protein;
                    fat += userProduct.Fat;
                    carbs += userProduct.Carbs;
                }
                else if (entry is UserDishViewModel userDish)
                {
                    calories += userDish.NutritionFacts.Calories;
                    protein += userDish.NutritionFacts.Protein;
                    fat += userDish.NutritionFacts.Fat;
                    carbs += userDish.NutritionFacts.Carbs;
                }
            }

            TotalNutrition = new(calories, protein, fat, carbs);
        }

        private void UpdateTotalQuantity()
        {
            double totalQuantity = 0;

            foreach (var entry in AllEntries)
            {
                if (entry is UserProductViewModel userProduct) totalQuantity += userProduct.Quantity;
                else if (entry is UserDishViewModel userDish) totalQuantity += userDish.Quantity;
            }

            TotalQuantity = totalQuantity;
            FormattedTotalQuantity = $"{TotalQuantity} г";
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
        #endregion

        private async Task LoadDataForDateAsync(DateTime date)
        {
            try
            {
                var userMealEntriesDto = await _apiService.GetUserMealsForDate(date);
                if (userMealEntriesDto is null) return;

                AllEntries.Clear();

                foreach (var userMealEntryDto in userMealEntriesDto)
                {
                    // отдельный продукт
                    if (!userMealEntryDto.UserDishId.HasValue)
                    {
                        var product = userMealEntryDto.Ingredients.FirstOrDefault();
                        if (product is null) continue;

                        var userProductViewModel = new UserProductViewModel(
                            product.UserProductId, product.ProductNameSnapshot,
                            quantity: (double)userMealEntryDto.TotalQuantity,
                            totalNutrition: userMealEntryDto.TotalNutrition)
                        {
                            MealEntryId = userMealEntryDto.Id,
                            MealType = userMealEntryDto.MealType
                        };

                        AllEntries.Add(userProductViewModel);
                    }
                    else // блюдо
                    {
                        var userDish = await _apiService.GetUserDishAsync(userMealEntryDto.UserDishId.Value);
                        if (userDish is null) continue;

                        var userDishViewModel = new UserDishViewModel(
                            _apiService, userDish.Id, userDish.Name,
                             isReadyDish: userDish.IsReadyDish,
                             quantity: (double)userMealEntryDto.TotalQuantity,
                             totalNutrition: userMealEntryDto.TotalNutrition,
                             ingredients: userMealEntryDto.Ingredients)
                        {
                            MealEntryId = userMealEntryDto.Id,
                            MealType = userMealEntryDto.MealType
                        };

                        AllEntries.Add(userDishViewModel);
                    }
                }

                OnPropertyChanged(nameof(HasEntries));
                OnPropertyChanged(nameof(entriesMessage));

                DistributeEntriesByMealType();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        private void DistributeEntriesByMealType()
        {
            BreakfastEntries.Clear();
            LunchEntries.Clear();
            DinnerEntries.Clear();
            SnackEntries.Clear();

            foreach (var entry in AllEntries)
            {
                var mealType = entry is UserProductViewModel userProductViewModel ? userProductViewModel.MealType
                    : entry is UserDishViewModel userDishViewModel ? userDishViewModel.MealType
                    : MealType.Snack;

                switch (mealType)
                {
                    case MealType.Breakfast: BreakfastEntries.Add(entry); break;
                    case MealType.Lunch: LunchEntries.Add(entry); break;
                    case MealType.Dinner: DinnerEntries.Add(entry); break;
                    case MealType.Snack: SnackEntries.Add(entry); break;
                }
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
        private static DateTime GetStartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public MainWindowViewModel(IApiService apiService, INotificationService notificationService) : base(apiService)
        {
            _apiService = apiService;
            _notificationService = notificationService;

            _currentWeekStart = GetStartOfWeek(SelectedDate);
            UpdateWeekDays();

            AllEntries.CollectionChanged += OnAllEntriesCollectionChanged;

            InitializeAsync();
        }

        #region Commands
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
            if (date == SelectedDate) return;
            SelectedDate = date;
        }

        [RelayCommand]
        private async Task AddUserProduct()
        {
            try
            {
                var userProductViewModel = await WeakReferenceMessenger.Default.Send(new AddUserProductMessage());
                if (userProductViewModel is null) return;

                var userMealEntryDto = new UserMealEntryDto()
                {
                    Date = SelectedDate,
                    Ingredients = new()
                    {
                        new UserMealEntryIngredientDto()
                        {
                            UserProductId = userProductViewModel.Id,
                            Quantity = (decimal)userProductViewModel.Quantity,
                            ProductNameSnapshot = userProductViewModel.Name!,
                            ProductNutritionInfoSnapshot = new NutritionInfo
                            {
                                Calories = userProductViewModel.Calories,
                                Protein = userProductViewModel.Protein,
                                Fat = userProductViewModel.Fat,
                                Carbs = userProductViewModel.Carbs
                            }
                        }
                    },
                    MealType = userProductViewModel.MealType,
                };

                var userMealEntry = await _apiService.AddUserMealEntryAsync(userMealEntryDto);

                if (userMealEntry is not null)
                    await LoadDataForDateAsync(SelectedDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task AddDish()
        {
            try
            {
                var userDishViewModel = await WeakReferenceMessenger.Default.Send(new AddUserDishMessage());
                if (userDishViewModel is null) return;

                var ingredients = userDishViewModel.Ingredients.Select(ingredient => new UserMealEntryIngredientDto()
                {
                    UserProductId = ingredient.UserProductId,
                    Quantity = (decimal)ingredient.Quantity,
                    ProductNameSnapshot = ingredient.ProductNameSnapshot,
                    ProductNutritionInfoSnapshot = ingredient.ProductNutritionInfoSnapshot
                }).ToList();

                var userMealEntryDto = new UserMealEntryDto()
                {
                    UserDishId = userDishViewModel.Id,
                    Date = SelectedDate,
                    Ingredients = ingredients,
                    TotalQuantity = (decimal)userDishViewModel.Quantity,
                    TotalNutrition = userDishViewModel.NutritionFacts,
                    MealType = userDishViewModel.MealType
                };

                var userMealEntry = await _apiService.AddUserMealEntryAsync(userMealEntryDto);

                if (userMealEntry is not null)
                    await LoadDataForDateAsync(SelectedDate);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RemoveEntry(object entry)
        {
            var mealEntryId = 0;

            if (entry is UserProductViewModel userProduct) mealEntryId = userProduct.MealEntryId;
            else if (entry is UserDishViewModel userDish) mealEntryId = userDish.MealEntryId;
            else return;

            if (await _apiService.DeleteUserMealEntryAsync(mealEntryId))
            {
                AllEntries.Remove(entry);
                UpdateTotals();
            }

        }

        [RelayCommand]
        private async Task SaveDay()
        {
            try
            {
                foreach (var entry in AllEntries)
                {
                    if (entry is UserProductViewModel userProductViewModel)
                    {
                        var userMealEntryProduct = new UserMealEntryDto()
                        {
                            Date = SelectedDate,
                            Ingredients = new()
                            {
                                new UserMealEntryIngredientDto()
                                {
                                    UserProductId = userProductViewModel.Id,
                                    Quantity = (decimal)userProductViewModel.Quantity,
                                    ProductNameSnapshot = userProductViewModel.Name!,
                                    ProductNutritionInfoSnapshot = new NutritionInfo
                                    {
                                        Calories = userProductViewModel.Calories,
                                        Protein = userProductViewModel.Protein,
                                        Fat = userProductViewModel.Fat,
                                        Carbs = userProductViewModel.Carbs
                                    }
                                }
                            },
                            MealType = userProductViewModel.MealType
                        };

                        await _apiService.UpdateUserMealEntry(userProductViewModel.MealEntryId, userMealEntryProduct);
                        userProductViewModel.IsDirty = false;
                    }
                    else if (entry is UserDishViewModel userDishViewModel)
                    {
                        var ingredients = new List<UserMealEntryIngredientDto>();

                        foreach (var ingredient in userDishViewModel.Ingredients)
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
                            UserDishId = userDishViewModel.Id,
                            Date = SelectedDate,
                            Ingredients = ingredients,
                            TotalQuantity = (decimal)userDishViewModel.Quantity,
                            TotalNutrition = userDishViewModel.NutritionFacts,
                            MealType = userDishViewModel.MealType
                        };

                        await _apiService.UpdateUserMealEntry(userDishViewModel.MealEntryId, userMealEntryDish);
                        userDishViewModel.IsDirty = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task OpenStatistics()
        {
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
            if (mainWindow is null) return;

            var statsViewModel = new StatsViewModel(_apiService, _notificationService);
            var statsWindow = new Views.StatsWindow()
            {
                DataContext = statsViewModel
            };
            await statsWindow.ShowDialog(mainWindow);
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
        #endregion
    }
}

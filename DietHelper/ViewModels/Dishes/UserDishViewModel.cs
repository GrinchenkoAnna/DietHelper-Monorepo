using Avalonia.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.MealEntries;
using DietHelper.Models.Messages;
using DietHelper.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class UserDishViewModel : ObservableValidator
    {
        private readonly IApiService _apiService;

        public ObservableCollection<UserDishIngredientViewModel> Ingredients { get; } = new();

        [ObservableProperty]
        private bool isReadyDish = false;

        [ObservableProperty]
        private bool canAddIngredients = true;

        private bool _isUpdatingIngredients = false;

        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private int userId;

        [ObservableProperty]
        [Required(ErrorMessage = "Название обязательно")]
        [MinLength(1, ErrorMessage = "Название не может быть пустым")]
        public string? name;

        [ObservableProperty]
        private NutritionInfo nutritionFacts = new();

        private NutritionInfo baseNutritionForReadyDish;

        [ObservableProperty]
        private int mealEntryId;

        [ObservableProperty]
        private MealType mealType;

        [ObservableProperty]
        private double quantity;
        [ObservableProperty]
        private string formattedQuantity;
        private bool isManualQuantity = false;

        [ObservableProperty]
        private bool isDirty = false;

        [ObservableProperty]
        private bool isInAddMode;

        public bool ShowDirtyIndicator => !IsInAddMode && IsDirty;
        partial void OnIsDirtyChanged(bool value) => OnPropertyChanged(nameof(ShowDirtyIndicator));
        partial void OnIsInAddModeChanged(bool value) => OnPropertyChanged(nameof(ShowDirtyIndicator));

        [ObservableProperty]
        public bool showIngredients = true;

        private void Recalculate()
        {
            if (IsReadyDish) return;

            Quantity = 0;

            var totalNutritionInfo = new NutritionInfo();
            foreach (var ingredient in Ingredients)
            {
                totalNutritionInfo.Calories += ingredient.CurrentNutrition.Calories;
                totalNutritionInfo.Protein += ingredient.CurrentNutrition.Protein;
                totalNutritionInfo.Fat += ingredient.CurrentNutrition.Fat;
                totalNutritionInfo.Carbs += ingredient.CurrentNutrition.Carbs;
            }

            NutritionFacts = totalNutritionInfo;

            UpdateTotalQuantity();
        }
        private void RecalculateForReadyDish()
        {
            if (!IsReadyDish || isManualQuantity) return;

            var factor = Quantity / 100.0;

            NutritionFacts = new NutritionInfo()
            {
                Calories = baseNutritionForReadyDish.Calories * factor,
                Protein = baseNutritionForReadyDish.Protein * factor,
                Fat = baseNutritionForReadyDish.Fat * factor,
                Carbs = baseNutritionForReadyDish.Carbs * factor
            };

            FormattedQuantity = $"{Quantity:F0}";
        }

        private void RestoreBaseNutritionForReadyDish(NutritionInfo totalNutrition, double quantity)
        {
            baseNutritionForReadyDish = new NutritionInfo()
            {
                Calories = totalNutrition.Calories * 100 / quantity,
                Protein = totalNutrition.Protein * 100 / quantity,
                Fat = totalNutrition.Fat * 100 / quantity,
                Carbs = totalNutrition.Carbs * 100 / quantity
            };
        }

        public UserDishViewModel() { }

        // загрузка из справочника
        public UserDishViewModel(UserDish userDish, IApiService apiService)
        {
            _apiService = apiService;

            Id = userDish.Id;
            UserId = userDish.UserId;
            Name = userDish.Name ?? string.Empty;
            IsReadyDish = userDish.IsReadyDish;
            CanAddIngredients = !IsReadyDish;

            if (userDish.IsReadyDish)
            {
                baseNutritionForReadyDish = new NutritionInfo()
                {
                    Calories = userDish.NutritionFacts?.Calories ?? 0,
                    Protein = userDish.NutritionFacts?.Protein ?? 0,
                    Fat = userDish.NutritionFacts?.Fat ?? 0,
                    Carbs = userDish.NutritionFacts?.Carbs ?? 0
                };
                Quantity = 100;
                RecalculateForReadyDish();
            }
            else
            {
                foreach (var ingredient in userDish.Ingredients)
                {
                    var ingredientViewModel = new UserDishIngredientViewModel(ingredient);
                    Ingredients.Add(ingredientViewModel);
                    ingredientViewModel.PropertyChanged += OnIngredientPropertyChanged;
                }
                Recalculate();
                Quantity = userDish.Quantity;
            }

            Ingredients.CollectionChanged += async (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (UserDishIngredientViewModel item in e.NewItems)
                        item.PropertyChanged += OnIngredientPropertyChanged;

                if (e.OldItems != null)
                    foreach (UserDishIngredientViewModel item in e.OldItems)
                        item.PropertyChanged -= OnIngredientPropertyChanged;

                Recalculate();
            };

            IsDirty = false;
            IsInAddMode = true;
        }

        // загрузка из истории
        public UserDishViewModel(IApiService apiService, int id, string name, bool isReadyDish, double quantity,
                                NutritionInfo totalNutrition, IEnumerable<UserMealEntryIngredientDto>? ingredients = null)
        {
            _apiService = apiService;
            Id = id;
            Name = name;
            IsReadyDish = isReadyDish;
            CanAddIngredients = !IsReadyDish;
            NutritionFacts = totalNutrition;

            if (!IsReadyDish)
            {
                if (ingredients.Count() != 0)
                {
                    foreach (var ingredient in ingredients)
                    {
                        var ingredientViewModel = new UserDishIngredientViewModel()
                        {
                            Id = ingredient.Id,
                            UserProductId = ingredient.UserProductId,
                            UserDishId = id,
                            Name = ingredient.ProductNameSnapshot,
                            ProductNameSnapshot = ingredient.ProductNameSnapshot,
                            ProductNutritionInfoSnapshot = ingredient.ProductNutritionInfoSnapshot,
                            Quantity = (double)ingredient.Quantity
                        };
                        Ingredients.Add(ingredientViewModel);
                        ingredientViewModel.PropertyChanged += OnIngredientPropertyChanged;
                    }
                    UpdateTotalQuantity();
                }
                else
                {
                    Quantity = 0;
                    Recalculate();
                }
            }
            else
            {

                if (quantity != 100)
                {
                    isManualQuantity = true;
                    RestoreBaseNutritionForReadyDish(totalNutrition, quantity);
                    Quantity = quantity;
                    isManualQuantity = false;
                }
                else
                {
                    baseNutritionForReadyDish = totalNutrition;
                    Quantity = quantity;
                }
            }

            Ingredients.CollectionChanged += async (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (UserDishIngredientViewModel item in e.NewItems)
                        item.PropertyChanged += OnIngredientPropertyChanged;

                if (e.OldItems != null)
                    foreach (UserDishIngredientViewModel item in e.OldItems)
                        item.PropertyChanged -= OnIngredientPropertyChanged;

                Recalculate();
            };

            IsDirty = false;
            IsInAddMode = false;
        }

        private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isUpdatingIngredients) return;

            if (e.PropertyName == nameof(UserDishIngredientViewModel.Quantity))
            {
                IsDirty = true;
                Recalculate();
            }
        }

        partial void OnQuantityChanged(double value)
        {
            if (IsReadyDish)
            {
                IsDirty = true;
                RecalculateForReadyDish();
            }
        }

        private void UpdateTotalQuantity()
        {
            if (!IsReadyDish)
            {
                Quantity = Ingredients.Sum(ingredient => ingredient.Quantity);
            }
            FormattedQuantity = $"{Quantity:F0}";
        }

        [RelayCommand]
        private void ApplyPortion()
        {
            if (IsReadyDish || Ingredients.Count == 0) return;

            double currentQuantity = Ingredients.Sum(i => i.Quantity);
            if (currentQuantity == 0 || currentQuantity <= 0) return;

            double inputQuantity = 0;
            double.TryParse(FormattedQuantity, out inputQuantity);
            double factor = inputQuantity / currentQuantity;

            _isUpdatingIngredients = true;
            foreach (var ingredient in Ingredients)
                ingredient.Quantity *= factor;
            _isUpdatingIngredients = false;

            IsDirty = true;

            Recalculate();
        }

        [RelayCommand]
        private async Task AddDishIngredient()
        {
            var userDishIngredientViewModel = await WeakReferenceMessenger.Default.Send(new AddDishIngredientMessage());

            if (userDishIngredientViewModel is null) return;

            if (Id <= 0)
            {
                Debug.WriteLine($"[UserDishViewModel] DishId is invalid: {Id}");
                return;
            }

            if (Ingredients.Any(i => i.UserProductId == userDishIngredientViewModel.UserProductId)) return;

            userDishIngredientViewModel.UserDishId = Id;
            var userDishIngredient = userDishIngredientViewModel.ToModel();
            var newIngredientId = await _apiService.AddUserDishIngredientAsync(Id, userDishIngredient);

            if (newIngredientId.HasValue)
            {
                userDishIngredientViewModel.Id = newIngredientId.Value;
                Ingredients.Add(userDishIngredientViewModel);
                IsDirty = true;
            }
        }

        [RelayCommand]
        private async Task RemoveIngredient(UserDishIngredientViewModel userDishIngredientViewModel)
        {
            var isSuccess = await _apiService.RemoveUserDishIngredientAsync(Id, userDishIngredientViewModel.Id);
            if (isSuccess)
            {
                Ingredients.Remove(userDishIngredientViewModel);
                IsDirty = true;
            }
        }
    }
}


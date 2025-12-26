using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Products;
using System;
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
        private readonly UserDish _model;
        private readonly NutritionCalculator _calculator;
        private readonly ApiService _apiService;

        public ObservableCollection<UserDishIngredientViewModel> Ingredients { get; } = new();

        public ObservableCollection<UserProductViewModel> DisplayedIngredients { get; } = new();

        private bool _updatingFromDisplayProducts = false;

        private bool IsManual = false;

        [ObservableProperty]
        private int id = -1;

        [ObservableProperty]
        private int userId = -1;

        [ObservableProperty]
        [Required(ErrorMessage = "Название обязательно")]
        [MinLength(1, ErrorMessage = "Название не может быть пустым")]
        public string? name;

        [ObservableProperty]
        private NutritionInfo nutritionFacts = new();

        public double Calories
        {
            get => NutritionFacts.Calories;
            set => NutritionFacts.Calories = value;
        }

        public double Protein
        {
            get => NutritionFacts.Protein;
            set => NutritionFacts.Protein = value;
        }

        public double Fat
        {
            get => NutritionFacts.Fat;
            set => NutritionFacts.Fat = value;
        }

        public double Carbs
        {
            get => NutritionFacts.Carbs;
            set => NutritionFacts.Carbs = value;
        }

        [ObservableProperty]
        private double quantity;
        [ObservableProperty]
        private string formattedQuantity;
        partial void OnQuantityChanged(double value) => UpdateTotalQuantity();
        private void UpdateTotalQuantity()
        {
            if (!IsManual)
            {
                Quantity = Ingredients.Sum(ingredient => ingredient.Quantity);
            }

            FormattedQuantity = $"{Quantity} г";
        }

        private void HandleProductChanged(object? sender, EventArgs e)
        {
            OnIngredientsChanged();
        }

        private void OnIngredientsChanged() => Recalculate();

        private async void Recalculate(UserDishIngredientViewModel? changedProduct = null)
        {
            var dishIngredients = Ingredients
            .Select(ingredient => new IngredientCalculationDto(
                ingredient.UserProductId,
                ingredient.Quantity))
            .ToList();

            NutritionFacts = await _calculator.CalculateUserDishNutrition(dishIngredients);

            UpdateTotalQuantity();
        }

        private async Task SyncDisplayProducts()
        {
            foreach (var product in DisplayedIngredients)
                product.ProductChanged -= OnDisplayProductChanged;

            DisplayedIngredients.Clear();

            foreach (var ingredient in Ingredients)
            {
                User user = await _apiService.GetUserAsync();
                
                var userProduct = await _apiService.GetUserProductAsync(ingredient.UserProductId);                

                var userProductViewModel = new UserProductViewModel(userProduct)
                {
                    Quantity = ingredient.Quantity
                };

                userProductViewModel.ProductChanged += OnDisplayProductChanged;

                DisplayedIngredients.Add(userProductViewModel);
            }
        }
        private async void OnDisplayProductChanged(object? sender, EventArgs e)
        {
            if (_updatingFromDisplayProducts) return;

            _updatingFromDisplayProducts = true;

            try
            {
                if (sender is UserProductViewModel product)
                {
                    var ingredient = Ingredients.FirstOrDefault(i => i.UserProductId == product.Id);
                    if (ingredient != null && Math.Abs(ingredient.Quantity - product.Quantity) > 0.001)
                    {
                        ingredient.Quantity = product.Quantity;

                        await UpdateModelAsync();
                        //await _apiService.UpdateUserDishAsync(_model);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении из UI: {ex.Message}");
            }
            finally
            {
                _updatingFromDisplayProducts = false;
            }
        }

        public UserDishViewModel() { }

        public UserDishViewModel(UserDish userDish, NutritionCalculator calculator, ApiService apiService, bool isManual = false)
        {
            _model = userDish;
            _calculator = calculator;
            _apiService = apiService;

            IsManual = isManual;
            Id = userDish.Id;
            UserId = userDish.UserId;
            Name = userDish.Name ?? string.Empty;

            foreach (var ingredient in userDish.Ingredients)
            {
                Ingredients.Add(new UserDishIngredientViewModel(ingredient));
            }            

            Ingredients.CollectionChanged += async (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<UserDishIngredientViewModel>())
                    {
                        SetupIngredientSubscription(item);
                    }
                    _ = SyncDisplayProducts();
                }                

                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<UserDishIngredientViewModel>())
                    {
                        item.PropertyChanged -= OnIngredientPropertyChanged;
                    }
                    _ = SyncDisplayProducts();
                }

                Recalculate();
                //_ = SyncDisplayProducts();
            };

            foreach (var ingredient in Ingredients)
            {
                SetupIngredientSubscription(ingredient);
            }

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(Calories) or nameof(Protein)
                    or nameof(Fat) or nameof(Carbs))
                    Recalculate();
            };

            _ = Task.Run(async () => await SyncDisplayProducts());
            //_ = SyncDisplayProducts();
            Recalculate();
        }

        private void SetupIngredientSubscription(UserDishIngredientViewModel ingredient)
        {
            ingredient.PropertyChanged -= OnIngredientPropertyChanged;
            ingredient.PropertyChanged += OnIngredientPropertyChanged;
        }

        private void SetupIngredientSubscriptions()
        {
            foreach (var ingredient in Ingredients)
                SetupIngredientSubscription(ingredient);
        }

        private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserDishIngredientViewModel.Quantity))
            {
                Recalculate();
                if (!_updatingFromDisplayProducts)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SyncDisplayProducts();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при обновлении продуктов: {ex.Message}");
                        }
                    });
                }
            }

        }

        [RelayCommand]
        private async Task AddDishIngredient()
        {
            var userDishIngredientViewModel = await WeakReferenceMessenger.Default.Send(new AddDishIngredientMessage());

            if (userDishIngredientViewModel is not null)
            {
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
                    SetupIngredientSubscription(userDishIngredientViewModel);

                    UpdateLocalModel(userDishIngredientViewModel);

                    Recalculate();
                    //await SyncDisplayProducts();
                }                
            }
        }

        [RelayCommand]
        private async Task RemoveIngredient(UserDishIngredientViewModel userDishIngredientViewModel)
        {
            if (Ingredients.Contains(userDishIngredientViewModel))
            {
                var result = await _apiService.RemoveUserDishIngredientAsync(Id, userDishIngredientViewModel.Id);

                if (result)
                {
                    userDishIngredientViewModel.PropertyChanged -= OnIngredientPropertyChanged;
                    Ingredients.Remove(userDishIngredientViewModel);

                    var ingredientToRemove = _model.Ingredients.FirstOrDefault(i => i.Id == userDishIngredientViewModel.Id);
                    if (ingredientToRemove is not null)
                    {
                        _model.Ingredients.Remove(ingredientToRemove);
                        _model.UpdateNutritionFromIngredients();
                    }

                    Recalculate();
                    await SyncDisplayProducts();
                }
            }
        }

        private void UpdateLocalModel(UserDishIngredientViewModel userDishIngredientViewModel)
        {
            var ingredient = new UserDishIngredient
            {
                Id = userDishIngredientViewModel.Id,
                UserDishId = _model.Id,
                UserProductId = userDishIngredientViewModel.UserProductId,
                Quantity = userDishIngredientViewModel.Quantity,
                IsDeleted = false
            };

            _model.Ingredients.Add(ingredient);
            _model.UpdateNutritionFromIngredients();
        }

        private async Task UpdateModelAsync()
        {
            //пересоздание ингредиентов из ViewModel - так можно?
            _model.Ingredients.Clear();

            foreach (var ingredientVm in Ingredients)
            {
                _model.Ingredients.Add(new UserDishIngredient
                {
                    Id = ingredientVm.Id,
                    UserDishId = _model.Id,
                    UserProductId = ingredientVm.UserProductId,
                    Quantity = ingredientVm.Quantity,
                    IsDeleted = false
                });
            }

            _model.UpdateNutritionFromIngredients();
        }        
    }
}


using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Products;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class UserDishViewModel : ObservableValidator
    {
        private readonly UserDish _model;
        private readonly NutritionCalculator _calculator;
        private readonly DatabaseService _dbService;

        public ObservableCollection<UserDishIngredientViewModel> Ingredients { get; } = new();

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

        private void Recalculate(UserDishIngredientViewModel? changedProduct = null)
        {
            var dishIngredients = Ingredients
            .Select(ingredient => new IngredientCalculationDto(
                ingredient.UserProductId,
                ingredient.Quantity))
            .ToList();

            if (!IsManual)
            {
                NutritionFacts = _calculator.CalculateDishNutrition(dishIngredients);
            }                
            
            UpdateTotalQuantity();           
        }

        public UserDishViewModel() {}

        public UserDishViewModel(UserDish userDish, NutritionCalculator calculator, bool isManual = false)
        {
            _model = userDish;
            _calculator = calculator;
            //_dbService = new DatabaseService();

            IsManual = isManual;      
            Id = userDish.Id;
            UserId = userDish.UserId;
            Name = userDish.Name ?? string.Empty;

            Ingredients.CollectionChanged += (s, e) => Recalculate();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(Calories) or nameof(Protein)
                    or nameof(Fat) or nameof(Carbs))
                {
                    Recalculate();
                }
            };

            Recalculate();
        }

        private void SetupIngredientSubscriptions()
        {
            foreach (var ingredient in Ingredients)
            {
                ingredient.PropertyChanged -= OnIngredientPropertyChanged;
                ingredient.PropertyChanged += OnIngredientPropertyChanged;
            }
        }

        private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserDishIngredientViewModel.Quantity))
                Recalculate();
        }

        [RelayCommand]
        private async Task AddDishIngredient()
        {
            var ingredient = await WeakReferenceMessenger.Default.Send(new AddDishIngredientMessage());

            if (ingredient is not null)
            {
                if (Ingredients.Any(i => i.Id == ingredient.Id)) return;

                Ingredients.Add(ingredient);
                SetupIngredientSubscriptions();
                Recalculate();

                //Обновить модель через API
                //await UpdateModelAsync();
                //await _apiService.UpdateUserDishAsync(_model);
            }
        }

        [RelayCommand]
        private async Task RemoveIngredient(UserDishIngredientViewModel ingredient)
        {
            if (Ingredients.Contains(ingredient))
            {
                ingredient.PropertyChanged -= OnIngredientPropertyChanged; // Отписываемся
                Ingredients.Remove(ingredient);
                Recalculate();

                //Обновить модель через API
                //await UpdateModelAsync();
                //await _apiService.UpdateUserDishAsync(_model);
            }
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


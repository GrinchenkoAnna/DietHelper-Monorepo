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
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class DishViewModel : ObservableValidator
    {
        //класс будет удален. Все закомментированные строки кода уже не используются
        private readonly Dish _model;
        private readonly NutritionCalculator _calculator;
        private readonly DatabaseService _dbService;

        private Dictionary<ProductViewModel, NutritionInfo> _ingredinetNutrition = new();

        public ObservableCollection<ProductViewModel> Ingredients { get; } = new();

        private bool IsManual = false;

        [ObservableProperty]
        private int id = -1;

        [ObservableProperty]
        private NutritionInfo nutritionFacts = new();

        [ObservableProperty]
        private double quantity;
        [ObservableProperty]
        private string formattedQuantity;
        partial void OnQuantityChanged(double value)
        {
            UpdateTotalQuantity();
        }
        private void UpdateTotalQuantity()
        {
            if (!IsManual)
            {
                Quantity = Ingredients.Sum(ingredient => ingredient.Quantity);
            }
                
            FormattedQuantity = $"{Quantity} г";
        }        

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        private void HandleProductChanged(object? sender, EventArgs e)
        {
            OnIngredientsChanged();
        }

        private void OnIngredientsChanged() => Recalculate();

        private void Recalculate(ProductViewModel? changedProduct = null)
        {
            var dishIngredients = Ingredients
            .Select(product => new DishIngredient
            {
                Ingredient = product.ToModel(),
                Quantity = product.Quantity
            })
            .ToList();

            if (!IsManual)
            {
                //NutritionFacts = _calculator.CalculateDishNutrition(dishIngredients);
            }                
            
            UpdateTotalQuantity();           
        }

        public DishViewModel() {}

        public DishViewModel(Dish dish, NutritionCalculator calculator, bool isManual = false)
        {
            _model = dish;
            _calculator = calculator;
            // планируется сделать через DI
            _dbService = new DatabaseService();
            IsManual = isManual;      
            id = dish.Id;

            Ingredients.CollectionChanged += (sender, e) =>
            {
                if (e.OldItems is not null && e.OldItems.Count > 0)
                {
                    foreach (var item in e.OldItems.OfType<ProductViewModel>())
                    {
                        item.ProductChanged -= HandleProductChanged;
                    }
                }

                if (e.NewItems is not null && e.NewItems.Count > 0)
                {
                    foreach (var item in e.NewItems.OfType<ProductViewModel>())
                    {
                        item.ProductChanged += HandleProductChanged;
                    }
                }
            };

            foreach (var item in dish.Ingredients)
            {
                var productViewModel = new ProductViewModel(item.Ingredient);
                Ingredients.Add(productViewModel);
                productViewModel.ProductChanged += HandleProductChanged;
            }

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
                IsManual = false;
                Recalculate();
                await UpdateModelAsync();
                await _dbService.UpdateDishAsync(_model);
            }
        }

        [RelayCommand]
        private async Task RemoveIngredient(ProductViewModel product)
        {
            if (Ingredients.Contains(product))
            {
                product.ProductChanged -= HandleProductChanged;                
                Ingredients.Remove(product);                
                Recalculate();
                await UpdateModelAsync();
                await _dbService.UpdateDishAsync(_model);
            }            
        }

        private async Task UpdateModelAsync()
        {
            _model.Ingredients.Clear();

            var productsIds = Ingredients.Select(p => p.Id).ToList();

            if (productsIds.Count == 0) return;

            var products = await _dbService.GetProductsByIdAsync(productsIds);

            foreach (var productVm in Ingredients)
            {
                if (products.TryGetValue(productVm.Id, out var product))
                {
                    _model.Ingredients.Add(new DishIngredient()
                    {
                        ProductId = productVm.Id,
                        //Ingredient = product, 
                        Quantity = productVm.Quantity
                    });
                }                
            }
        }
    }
}

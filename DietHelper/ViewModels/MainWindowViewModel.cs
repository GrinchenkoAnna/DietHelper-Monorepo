using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        //private readonly DatabaseService _dbService;
        private readonly ApiService _apiService;

        private ObservableCollection<ProductViewModel> _products = [];
        public ObservableCollection<ProductViewModel> Products
        {
            get => _products;
            set
            {
                if (_products is not null)
                {
                    _products.CollectionChanged -= OnProductsCollectionChanged;
                    UnsubscribeFromProducts(_products);
                }

                SetProperty(ref _products, value);

                if (_products is not null)
                {
                    _products.CollectionChanged += OnProductsCollectionChanged; 
                    SubscribeToProducts(_products);
                }

                UpdateTotals();
            }
        }

        private ObservableCollection<DishViewModel> _dishes = [];
        public ObservableCollection<DishViewModel> Dishes
        {
            get => _dishes;
            set
            {
                if (_dishes is not null)
                {
                    _dishes.CollectionChanged -= OnDishesCollectionChanged;
                    UnsubscribeFromDishes(_dishes);
                }

                SetProperty(ref _dishes, value);

                if (_dishes is not null)
                {
                    _dishes.CollectionChanged += OnDishesCollectionChanged;
                    SubscribeToDishes(_dishes);
                }

                UpdateTotals();
            }
        }

        private void SubscribeToProducts(IEnumerable<ProductViewModel> products)
        {
            foreach (var product in products) 
                product.PropertyChanged += OnProductPropertyChanged;
        }
        private void UnsubscribeFromProducts(IEnumerable<ProductViewModel> products)
        {
            foreach (var product in products)
                product.PropertyChanged -= OnProductPropertyChanged;
        }
        private void OnProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductViewModel.Quantity) ||
                e.PropertyName == nameof(ProductViewModel.TotalNutritionInfo))
                UpdateTotals();
        }

        private void SubscribeToDishes(IEnumerable<DishViewModel> dishes)
        {
            foreach (var dish in dishes)
                dish.PropertyChanged += OnDishPropertyChanged;
        }
        private void UnsubscribeFromDishes(IEnumerable<DishViewModel> dishes)
        {
            foreach (var dish in dishes)
                dish.PropertyChanged -= OnDishPropertyChanged;
        }
        private void OnDishPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DishViewModel.Quantity) ||
                e.PropertyName == nameof(DishViewModel.NutritionFacts))
                UpdateTotals();
        }

        private void OnProductsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
                SubscribeToProducts(e.NewItems.Cast<ProductViewModel>());

            if (e.OldItems is not null)
                UnsubscribeFromProducts(e.OldItems.Cast<ProductViewModel>());

            UpdateTotals();
        }
        private void OnDishesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
                SubscribeToDishes(e.NewItems.Cast<DishViewModel>());

            if (e.OldItems is not null)
                UnsubscribeFromDishes(e.OldItems.Cast<DishViewModel>());

            UpdateTotals();
        }

        private async void LoadMocksFromServerAsync()
        {
            try
            {
                var product = await _apiService.GetProductMockAsync();
                Products.Add(new ProductViewModel(product));

                //+ блюда
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных с сервера: {ex.Message}");
            }
        }

        public MainWindowViewModel() : this(new ApiService()) { }

        public MainWindowViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoadMocksFromServerAsync();

            _products.CollectionChanged += OnProductsCollectionChanged;
            _dishes.CollectionChanged += OnDishesCollectionChanged;

            SubscribeToProducts(_products);
            SubscribeToDishes(_dishes);

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
            var totalProductCalories = Products.Sum(p => p.TotalNutritionInfo.Calories);
            var totalProductProtein = Products.Sum(p => p.TotalNutritionInfo.Protein);
            var totalProductFat = Products.Sum(p => p.TotalNutritionInfo.Fat);
            var totalProductCarbs = Products.Sum(p => p.TotalNutritionInfo.Carbs);

            var totalDishCalories = Dishes.Sum(d => d.NutritionFacts.Calories);
            var totalDishProtein = Dishes.Sum(d => d.NutritionFacts.Protein);
            var totalDishFat = Dishes.Sum(d => d.NutritionFacts.Fat);
            var totalDishCarbs = Dishes.Sum(d => d.NutritionFacts.Carbs);

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
            var productQuantity = Products.Sum(p => p.Quantity);
            var dishQuantity = Dishes.Sum(d => d.Quantity);

            TotalQuantity = productQuantity + dishQuantity;
            FormattedTotalQuantity = $"{TotalQuantity} г";
        }

        [RelayCommand]
        private async Task AddProduct()
        {
            var product = await WeakReferenceMessenger.Default.Send(new AddProductMessage());
            if (product is not null) Products.Add(product);
        }

        [RelayCommand]
        private async Task RemoveProduct(ProductViewModel product)
        {            
            Products.Remove(product);
        }

        [RelayCommand]
        private async Task AddDish()
        {
            var dish = await WeakReferenceMessenger.Default.Send(new AddDishMessage());
            if (dish is not null) Dishes.Add(dish);
        }

        [RelayCommand]
        private void RemoveDish(DishViewModel dish)
        {
            Dishes.Remove(dish);
        }
    }
}

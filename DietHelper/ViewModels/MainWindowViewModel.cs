using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
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
        private readonly ApiService _apiService;

        private ObservableCollection<UserProductViewModel> _userProducts = [];

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

        //private ObservableCollection<ProductViewModel> _products = [];
        //public ObservableCollection<ProductViewModel> Products
        //{
        //    get => _products;
        //    set
        //    {
        //        if (_products is not null)
        //        {
        //            _products.CollectionChanged -= OnProductsCollectionChanged;
        //            UnsubscribeFromProducts(_products);
        //        }

        //        SetProperty(ref _products, value);

        //        if (_products is not null)
        //        {
        //            _products.CollectionChanged += OnProductsCollectionChanged; 
        //            SubscribeToProducts(_products);
        //        }

        //        UpdateTotals();
        //    }
        //}

        //private ObservableCollection<DishViewModel> _dishes = [];
        //public ObservableCollection<DishViewModel> Dishes
        //{
        //    get => _dishes;
        //    set
        //    {
        //        if (_dishes is not null)
        //        {
        //            _dishes.CollectionChanged -= OnDishesCollectionChanged;
        //            UnsubscribeFromDishes(_dishes);
        //        }

        //        SetProperty(ref _dishes, value);

        //        if (_dishes is not null)
        //        {
        //            _dishes.CollectionChanged += OnDishesCollectionChanged;
        //            SubscribeToDishes(_dishes);
        //        }

        //        UpdateTotals();
        //    }
        //}

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

        private async void LoadMocksFromServerAsync()
        {
            try
            {
                var userProduct = await _apiService.GetUserProductMockAsync();
                 if (userProduct is not null) UserProducts.Add(new UserProductViewModel(userProduct));

                //+ блюда
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных с сервера: {ex.Message}");
            }
        }

        public MainWindowViewModel() : this(new ApiService()) { }

        public MainWindowViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            //LoadMocksFromServerAsync();

            #region Mocks for debugging bindings
            var baseProductMock = new BaseProductViewModel
            {
                Id = 1,
                Name = "Молоко 2,5%",
                Calories = 45.0,
                Protein = 2.9,
                Fat = 2.5,
                Carbs = 4.8
            };

            var userProductMock = new UserProductViewModel(
                new UserProduct
                {
                    Id = 1,
                    UserId = 1,
                    BaseProductId = 1,
                    CustomNutrition = new NutritionInfo
                    {
                        Calories = 45.0,
                        Protein = 2.9,
                        Fat = 2.5,
                        Carbs = 4.8
                    },
                    IsDeleted = false
                });
            string name = userProductMock.Name;
            UserProducts.Add(userProductMock);

            var ingredientMock = new UserDishIngredientViewModel
            {
                Id = 1,
                UserDishId = 1,
                UserProductId = 1,
                Name = "Молоко 2,5%",
                Quantity = 200,
                CurrentNutrition = new NutritionInfo
                {
                    Calories = 90.0,
                    Protein = 5.8,
                    Fat = 5.0,
                    Carbs = 9.6
                }
            };
            
            var dishMock = new UserDishViewModel(
                new UserDish
                {
                    Id = 1,
                    UserId = 1,
                    Name = "Куриный суп",
                    NutritionFacts = new NutritionInfo
                    {
                        Calories = 350.0,
                        Protein = 25.0,
                        Fat = 12.0,
                        Carbs = 30.0
                    },
                    IsDeleted = false
                },
                new NutritionCalculator(new ApiService()),
                new ApiService(),
                isManual: false);

            dishMock.Ingredients.Add(ingredientMock);
            dishMock.Ingredients.Add(new UserDishIngredientViewModel
            {
                Id = 2,
                UserDishId = 1,
                UserProductId = 2,
                Name = "Курица",
                Quantity = 150,
                CurrentNutrition = new NutritionInfo
                {
                    Calories = 357.0,  // 238 * (150/100)
                    Protein = 27.3,
                    Fat = 27.6,
                    Carbs = 0
                }
            });
            UserDishes.Add(dishMock);
            #endregion

            //_products.CollectionChanged += OnProductsCollectionChanged;
            _userProducts.CollectionChanged += OnUserProductsCollectionChanged;
            _userDishes.CollectionChanged += OnUserDishesCollectionChanged;

            //SubscribeToProducts(_products);
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
            var userProduct = await WeakReferenceMessenger.Default.Send(new AddUserProductMessage());
            if (userProduct is not null) UserProducts.Add(userProduct);
        }

        [RelayCommand]
        private async Task RemoveUserProduct(UserProductViewModel userProduct)
        {            
            UserProducts.Remove(userProduct);
        }

        [RelayCommand]
        private async Task AddDish()
        {
            var userDish = await WeakReferenceMessenger.Default.Send(new AddUserDishMessage());
            if (userDish is not null) UserDishes.Add(userDish);
        }

        [RelayCommand]
        private void RemoveDish(UserDishViewModel userDish)
        {
            UserDishes.Remove(userDish);
        }
    }
}

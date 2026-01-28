using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
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
        private readonly INavigationService _navigationService;

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

        private async Task LoadDataFromServerAsync()
        {
            try
            {
                var userProduct = await _apiService.GetUserProductAsync(1);
                if (userProduct is not null)
                    UserProducts.Add(new UserProductViewModel(userProduct));

                var userDish = await _apiService.GetUserDishAsync(1);
                if (userDish is not null)
                {
                    var nutritionCalculator = new NutritionCalculator(_apiService);

                    var userDishViewModel = new UserDishViewModel(userDish, nutritionCalculator, _apiService);
                    UserDishes.Add(userDishViewModel);
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
                await LoadDataFromServerAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowWiewModel]: {ex.Message}");
            }
        }

        public MainWindowViewModel() : this(new ApiService(), null) { }

        public MainWindowViewModel(ApiService apiService, INavigationService navigationService) : base(apiService)
        {
            _apiService = apiService;
            _navigationService = navigationService;
            InitializeAsync();

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

        [RelayCommand]
        private async Task Logout()
        {
            try
            {
                await _apiService.LogoutAsync();
                await _navigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindowViewModel]: {ex.Message}");
            }
        }
    }
}

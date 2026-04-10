using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.Services;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Base
{
    public abstract partial class AddItemBaseViewModel<TModel, TViewModel> : ViewModelBase, IAddItemViewModel
        where TModel : class
        where TViewModel : class
    {
        protected readonly IApiService _apiService;
        protected readonly INotificationService _notificationService;

        [ObservableProperty] private string? searchText = string.Empty;
        [ObservableProperty] private bool isBusy = false;

        [ObservableProperty] private TViewModel? selectedUserItem;
        
        public ObservableCollection<TViewModel> UserSearchResults { get; } = new();

        protected ObservableCollection<TViewModel> AllUserItems { get; } = new();

        [ObservableProperty] protected string? manualName;
        [ObservableProperty] protected double manualCalories;
        [ObservableProperty] protected double manualProtein;
        [ObservableProperty] protected double manualFat;
        [ObservableProperty] protected double manualCarbs;
        
        protected abstract Task<TModel?> CreateNewUserItem();

        protected async Task<UserProduct?> CreateProductFromDataAsync(string name, double calories, double protein, double fat, double carbs, string? barcode = null)
        {
            BaseProduct? existingBaseProduct = null;

            if (!string.IsNullOrEmpty(barcode))
            {
                existingBaseProduct = await _apiService.GetBaseProductAsync(barcode);

                if (existingBaseProduct is not null)
                    return await CreateUserProductOnBaseProduct(calories, protein, fat, carbs, existingBaseProduct);
            }

            var baseProduct = new BaseProduct()
            {
                Name = name!,
                NutritionFacts = new NutritionInfo()
                {
                    Calories = calories,
                    Protein = protein,
                    Fat = fat,
                    Carbs = carbs
                },
                Barcode = barcode,
                IsDeleted = false
            };
            var createdBaseProduct = await _apiService.AddProductAsync(baseProduct);
            if (createdBaseProduct is null) return null;
            return await CreateUserProductOnBaseProduct(calories, protein, fat, carbs, createdBaseProduct);
        }

        private async Task<UserProduct?> CreateUserProductOnBaseProduct(double calories, double protein, double fat, double carbs, BaseProduct existingBaseProduct)
        {
            var userProduct = new UserProduct()
            {
                UserId = await GetCurrentUserId(),
                BaseProductId = existingBaseProduct.Id,
                CustomNutrition = new NutritionInfo()
                {
                    Calories = calories,
                    Protein = protein,
                    Fat = fat,
                    Carbs = carbs
                },
                IsDeleted = false
            };
            return await _apiService.AddUserProductAsync(userProduct);
        }

        protected void ClearManualEntries()
        {
            ManualName = string.Empty;
            ManualCalories = 0;
            ManualProtein = 0;
            ManualFat = 0;
            ManualCarbs = 0;
        }

        protected AddItemBaseViewModel(IApiService apiService, INotificationService notificationService) : base(apiService)
        {
            _apiService = apiService;
            _notificationService = notificationService;

            InitializeData();
        }

        protected abstract void InitializeData();

        partial void OnSearchTextChanged(string? value)
        {
            _ = DoSearch(SearchText);
        }

        protected async virtual Task DoSearch(string? term)
        {
            IsBusy = true;
            UserSearchResults.Clear();

            //временно
            foreach (var item in AllUserItems)
            {
                //не очень эффективный алгоритм поиска -> SearchIndex
                if (term is not null 
                    && item.GetType()
                           .GetProperty("Name")!
                           .GetValue(item)!
                           .ToString()
                           .Contains(term, System.StringComparison.CurrentCultureIgnoreCase))
                    UserSearchResults.Add(item);
            }

            IsBusy = false;
        }

        [RelayCommand]
        protected abstract void AddUserItem();

        [RelayCommand]
        protected abstract void AddManualItem();

        [RelayCommand]
        protected abstract void DeleteItemFromDatabase(TViewModel viewModel);

        protected void RemoveFromCollections(TViewModel viewModel)
        {
            UserSearchResults.Remove(viewModel);
            AllUserItems.Remove(viewModel);
        }
    }
}

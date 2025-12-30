using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Base
{
    public abstract partial class AddItemBaseViewModel<TModel, TViewModel> : ViewModelBase, IAddItemViewModel
        where TModel : class
        where TViewModel : class
    {
        protected readonly ApiService _apiService;

        [ObservableProperty] private string? searchText = string.Empty;
        [ObservableProperty] private bool isBusy = false;

        [ObservableProperty] private TViewModel? selectedUserItem;
        
        public ObservableCollection<TViewModel> UserSearchResults { get; } = new();

        protected ObservableCollection<TViewModel> AllUserItems { get; } = new();

        [ObservableProperty] private string? manualName;
        [ObservableProperty] private double manualCalories;
        [ObservableProperty] private double manualProtein;
        [ObservableProperty] private double manualFat;
        [ObservableProperty] private double manualCarbs;
        
        protected abstract Task<TModel> CreateNewUserItem();

        protected async Task<UserProduct> CreateProductAsync()
        {
            var baseProduct = new BaseProduct()
            {
                Name = ManualName!,
                NutritionFacts = new NutritionInfo()
                {
                    Calories = ManualCalories,
                    Protein = ManualProtein,
                    Fat = ManualFat,
                    Carbs = ManualCarbs
                },
                IsDeleted = false
            };
            var createdBaseProduct = await _apiService.AddProductAsync(baseProduct);

            User user = await GetCurrentUser();

            var userProduct = new UserProduct()
            {
                UserId = GetCurrentUserId(),
                User = user,
                BaseProductId = createdBaseProduct.Id,
                BaseProduct = createdBaseProduct,
                CustomNutrition = new NutritionInfo()
                {
                    Calories = ManualCalories,
                    Protein = ManualProtein,
                    Fat = ManualFat,
                    Carbs = ManualCarbs
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

        protected AddItemBaseViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
            InitializeData();
        }

        protected abstract void InitializeData();

        partial void OnSearchTextChanged(string? value)
        {
            _ = DoSearch(SearchText);
        }

        private async Task DoSearch(string? term)
        {
            IsBusy = true;
            UserSearchResults.Clear();

            //временно
            foreach (var item in AllUserItems)
            {
                //не очень эффективный алгоритм поиска
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
    }
}

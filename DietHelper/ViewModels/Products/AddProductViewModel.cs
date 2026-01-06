using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Products
{
    public partial class AddProductViewModel : AddItemBaseViewModel<UserProduct, UserProductViewModel>
    {
        public AddProductViewModel(ApiService _apiService) : base(_apiService) { }

        [ObservableProperty] private BaseProductViewModel? selectedBaseItem;

        public ObservableCollection<BaseProductViewModel> BaseSearchResults { get; } = new();

        public ObservableCollection<BaseProductViewModel> AllBaseItems { get; } = new();

        protected override async void InitializeData()
        {
            var userProducts = await _apiService.GetUserProductsAsync();

            foreach (var userProduct in userProducts)
            {
                if (userProduct.Id > 0)
                {
                    UserSearchResults.Add(new UserProductViewModel(userProduct));
                    AllUserItems.Add(new UserProductViewModel(userProduct));
                }
            }

            var baseProducts = await _apiService.GetBaseProductsAsync();

            foreach (var baseProduct in baseProducts)
            {
                if (baseProduct.Id > 0)
                {
                    BaseSearchResults.Add(new BaseProductViewModel(baseProduct));
                    AllBaseItems.Add(new BaseProductViewModel(baseProduct));
                }
            }
        }

        protected override async Task DoSearch(string? term)
        {
            IsBusy = true;

            UserSearchResults.Clear();
            BaseSearchResults.Clear();

            //UserProducts
            if (term is null || string.IsNullOrWhiteSpace(term))
                foreach (var item in AllUserItems) UserSearchResults.Add(item);
            else
            {
                foreach (var item in AllUserItems)
                {
                    if (item.GetType()
                           .GetProperty("Name")!
                           .GetValue(item)!
                           .ToString()
                           .Contains(term, System.StringComparison.CurrentCultureIgnoreCase))
                        UserSearchResults.Add(item);
                }
            }

            //BaseProducts
            if (term is null || string.IsNullOrWhiteSpace(term))
                foreach (var item in AllBaseItems) BaseSearchResults.Add(item);
            else
            {
                foreach (var item in AllBaseItems)
                {
                    if (item.GetType()
                           .GetProperty("Name")!
                           .GetValue(item)!
                           .ToString()
                           .Contains(term, System.StringComparison.CurrentCultureIgnoreCase))
                        BaseSearchResults.Add(item);
                }
            }

            IsBusy = false;
        }

        [RelayCommand]
        protected async Task AddBaseItem()
        {
            if (SelectedBaseItem is null) return;

            var user = await GetCurrentUser();

            var userProduct = new UserProduct()
            {
                UserId = user.Id,
                BaseProductId = SelectedBaseItem.Id,
                CustomNutrition = new NutritionInfo()
                {
                    Calories = SelectedBaseItem.Calories,
                    Protein = SelectedBaseItem.Protein,
                    Fat = SelectedBaseItem.Fat,
                    Carbs = SelectedBaseItem.Carbs
                }
            };
            var createdUserProduct = await _apiService.AddUserProductAsync(userProduct);
            var userProductViewModel = new UserProductViewModel(createdUserProduct)
            {
                Quantity = SelectedBaseItem.Quantity
            };
            WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(userProductViewModel));
        }

        protected override void AddUserItem()
        {
            if (SelectedUserItem is not null)
                WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(SelectedUserItem));
        }
        
        //UserProduct + BaseProduct
        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newUserProduct = await CreateNewUserItem();
            
            await _apiService.AddUserProductAsync(newUserProduct);

            var newItem = new UserProductViewModel(newUserProduct);

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(newItem));
        }

        protected override async Task<UserProduct> CreateNewUserItem()
        {
            return await CreateProductAsync();
        }
        
        protected override async void DeleteItemFromDatabase(UserProductViewModel userProductViewModel)
        {
            RemoveFromCollections(userProductViewModel);

            await _apiService.DeleteUserProductAsync(userProductViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserProductMessage(userProductViewModel.Id));
        }
    }
}

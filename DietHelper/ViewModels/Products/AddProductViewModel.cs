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

        private async Task DoGlobalSearch(string? term)
        {
            IsBusy = true;
            BaseSearchResults.Clear();

            //временно
            foreach (var item in AllBaseItems)
            {
                //не очень эффективный алгоритм поиска
                if (term is not null && item.GetType().GetProperty("Name")!.GetValue(item)!.ToString()
                    .Contains(term, System.StringComparison.CurrentCultureIgnoreCase))
                    BaseSearchResults.Add(item);
            }

            IsBusy = false;
        }

        [RelayCommand]
        protected void AddBaseItem()
        {
            if (SelectedBaseItem is not null)
                WeakReferenceMessenger.Default.Send(new AddBaseProductClosedMessage(SelectedBaseItem));
            //добавить base продукт в user
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

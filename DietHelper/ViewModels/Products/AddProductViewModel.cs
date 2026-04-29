using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Products
{
    public partial class AddProductViewModel : AddItemBaseViewModel<UserProduct, UserProductViewModel>
    {
        public AddProductViewModel(IApiService _apiService, INotificationService _notificationService) : base(_apiService, _notificationService) { }

        [ObservableProperty] private BaseProductViewModel? selectedBaseItem;

        [ObservableProperty] private string barcode = string.Empty;

        public ObservableCollection<BaseProductViewModel> BaseSearchResults { get; } = new();

        public ObservableCollection<BaseProductViewModel> AllBaseItems { get; } = new();

        protected override async void InitializeData()
        {
            var userProducts = await _apiService.GetUserProductsAsync();
            if (userProducts is not null)
            {
                foreach (var userProduct in userProducts)
                {
                    if (userProduct.Id > 0)
                    {
                        UserSearchResults.Add(new UserProductViewModel(userProduct));
                        AllUserItems.Add(new UserProductViewModel(userProduct));
                    }
                }
            }

            var baseProducts = await _apiService.GetBaseProductsAsync();
            if (baseProducts is not null)
            {
                foreach (var baseProduct in baseProducts)
                {
                    if (baseProduct.Id > 0)
                    {
                        BaseSearchResults.Add(new BaseProductViewModel(baseProduct));
                        AllBaseItems.Add(new BaseProductViewModel(baseProduct));
                    }
                }
            }
        }

        protected override async Task DoSearch(string? term)
        {
            IsBusy = true;

            var userResults = new List<UserProductViewModel>();
            var baseResults = new List<BaseProductViewModel>();

            await Task.Run(() =>
            {
                bool isTermEmpty = string.IsNullOrWhiteSpace(term);

                //UserProducts
                foreach (var item in AllUserItems)
                {
                    if (isTermEmpty || (item.Name ?? "").Contains(term!, StringComparison.CurrentCultureIgnoreCase))
                        userResults.Add(item);
                }

                //BaseProducts
                foreach (var item in AllBaseItems)
                {
                    if (isTermEmpty || (item.Name ?? "").Contains(term!, StringComparison.CurrentCultureIgnoreCase))
                        baseResults.Add(item);
                }
            });            

            UserSearchResults.Clear();
            foreach (var item in userResults) UserSearchResults.Add(item);
            BaseSearchResults.Clear();
            foreach(var item in baseResults) BaseSearchResults.Add(item);

            IsBusy = false;
        }

        [RelayCommand]
        protected async Task AddBaseItem()
        {
            if (SelectedBaseItem is null)
            {
                _notificationService.ShowError("Ошибка выбора продукта", "Не выбран продукт для добавления");
                return;
            }

            try
            {
                var createdUserProduct = await CreateUserProductFromBaseItemAsync(SelectedBaseItem);
                if (createdUserProduct is null)
                {
                    _notificationService.ShowError("Ошибка добавления", "Не удалось добавить продукт");
                    return;
                }

                var userProductViewModel = new UserProductViewModel(createdUserProduct)
                {
                    Name = SelectedBaseItem.Name,
                    Quantity = SelectedBaseItem.Quantity
                };
                WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(userProductViewModel));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AddProductViewModel]: {ex.Message}");
            }
        }

        protected override void AddUserItem()
        {
            if (SelectedUserItem is not null)
                WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(SelectedUserItem));
        }

        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newUserProduct = await CreateNewUserItem();
            if (newUserProduct is null)
            {
                _notificationService.ShowError("Создание продукта", "Не удалось создать продукт");
                return;
            }


            var newItem = new UserProductViewModel(newUserProduct)
            {
                Name = ManualName,
            };

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(newItem));
        }

        protected override async Task<UserProduct?> CreateNewUserItem()
        {
            return await CreateProductFromDataAsync(ManualName!, ManualCalories, ManualProtein, ManualFat, ManualCarbs);
        }

        [RelayCommand]
        private async Task FindProductInOpenFoodFacts()
        {
            await FindProductInOpenFoodFactsAsync(Barcode, BaseSearchResults);
        }

        protected override async void DeleteItemFromDatabase(UserProductViewModel userProductViewModel)
        {
            RemoveFromCollections(userProductViewModel);

            await _apiService.DeleteUserProductAsync(userProductViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserProductMessage(userProductViewModel.Id));
        }
    }
}

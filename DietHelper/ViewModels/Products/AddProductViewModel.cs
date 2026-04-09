using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.DTO;
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
            if (SelectedBaseItem is null)
            {
                _notificationService.ShowError("Ошибка выбора продукта", "Не выбран продукт для добавления");
                return;
            }

            UserProduct? createdUserProduct = null;

            // продукт из OpenFoodFacts
            if (SelectedBaseItem.Id <= 0 && !string.IsNullOrEmpty(SelectedBaseItem.Barcode))
            {
                createdUserProduct = await CreateProductFromDataAsync(
                    SelectedBaseItem.Name!,
                    SelectedBaseItem.Calories,
                    SelectedBaseItem.Protein,
                    SelectedBaseItem.Fat,
                    SelectedBaseItem.Carbs,
                    SelectedBaseItem.Barcode
                );

                if (createdUserProduct is null)
                {
                    _notificationService.ShowError("Ошибка добавления продукта", "Не удалось добавить продукт");
                    return;
                }
            }
            else // базовый продукт есть в бд
            {
                createdUserProduct = new UserProduct()
                {
                    BaseProductId = SelectedBaseItem.Id,
                    CustomNutrition = new NutritionInfo()
                    {
                        Calories = SelectedBaseItem.Calories,
                        Protein = SelectedBaseItem.Protein,
                        Fat = SelectedBaseItem.Fat,
                        Carbs = SelectedBaseItem.Carbs
                    }
                };

                if (createdUserProduct is null)
                {
                    _notificationService.ShowError("Ошибка добавления продукта", "Не удалось добавить продукт");
                    return;
                }

                createdUserProduct = await _apiService.AddUserProductAsync(createdUserProduct);
            }
            
            var userProductViewModel = new UserProductViewModel(createdUserProduct)
            {
                Name = SelectedBaseItem.Name,
                Quantity = SelectedBaseItem.Quantity
            };
            WeakReferenceMessenger.Default.Send(new AddUserProductClosedMessage(userProductViewModel));
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
        private async void FindProductInOpenFoodFacts()
        {
            if (string.IsNullOrWhiteSpace(Barcode))
            {
                _notificationService.ShowError("Ошибка заполнения штрих-кода", "Штрих-код не может быть пустым");
                return;
            }

            var openFoodFactsDto = await _apiService.GetProductFromOpenFoodFactsAsync(Barcode);

            if (openFoodFactsDto is null || !string.IsNullOrEmpty(openFoodFactsDto.Message))
            {
                _notificationService.ShowInfo("Поиск продукта по штрих-коду", "Продукт не найден");
                return;
            }

            BaseSearchResults.Clear();

            var baseProduct = new BaseProduct()
            {
                Name = openFoodFactsDto.Name,
                NutritionFacts = new NutritionInfo
                {
                    Calories = openFoodFactsDto.Calories,
                    Protein = openFoodFactsDto.Protein,
                    Fat = openFoodFactsDto.Fat,
                    Carbs = openFoodFactsDto.Carbs
                },
                Barcode = openFoodFactsDto.Barcode
            };

            BaseSearchResults.Add(new BaseProductViewModel(baseProduct));
        }
        
        protected override async void DeleteItemFromDatabase(UserProductViewModel userProductViewModel)
        {
            RemoveFromCollections(userProductViewModel);

            await _apiService.DeleteUserProductAsync(userProductViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserProductMessage(userProductViewModel.Id));
        }
    }
}

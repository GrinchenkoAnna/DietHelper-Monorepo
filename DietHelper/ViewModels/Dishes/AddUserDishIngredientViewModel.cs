using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using DietHelper.ViewModels.Products;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Dishes
{
    public partial class AddUserDishIngredientViewModel : AddItemBaseViewModel<UserProduct, UserProductViewModel>
    {
        public AddUserDishIngredientViewModel(IApiService _apiService, INotificationService _notificationService) : base(_apiService, _notificationService) { }

        [ObservableProperty] private BaseProductViewModel? selectedBaseItem;

        [ObservableProperty] private double quantity = 100;
        
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

        protected override async Task<UserProduct?> CreateNewUserItem()
        {
            return await CreateProductFromDataAsync(ManualName!, ManualCalories, ManualProtein, ManualFat, ManualCarbs);
        }

        protected override void AddUserItem()
        {
            if (SelectedUserItem is null) return;

            var ingredient = new UserDishIngredientViewModel
            {
                UserProductId = SelectedUserItem.Id,
                Name = SelectedUserItem.Name,
                Quantity = SelectedUserItem.Quantity,
                ProductNameSnapshot = SelectedUserItem.Name,
                ProductNutritionInfoSnapshot = new NutritionInfo()
                {
                    Calories = SelectedUserItem.NutritionFacts.Calories,
                    Protein = SelectedUserItem.NutritionFacts.Protein,
                    Fat = SelectedUserItem.NutritionFacts.Fat,
                    Carbs = SelectedUserItem.NutritionFacts.Carbs
                },
                CurrentNutrition = new NutritionInfo()
                {
                    Calories = SelectedUserItem.NutritionFacts.Calories,
                    Protein = SelectedUserItem.NutritionFacts.Protein,
                    Fat = SelectedUserItem.NutritionFacts.Fat,
                    Carbs = SelectedUserItem.NutritionFacts.Carbs
                }
            };

            WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(ingredient));
        }

        [RelayCommand]
        private async void AddBaseItem()
        {
            if (SelectedBaseItem is null)
            {
                _notificationService.ShowError("Ошибка выбора ингредиента", "Не выбран ингредиент для добавления");
                return;
            }

            UserProduct? createdUserProduct = null;

            try
            {                
                // Продукт из OpenFoodFacts (временный, Id <= 0)
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
                        _notificationService.ShowError("Ошибка добавления", "Не удалось создать продукт из OpenFoodFacts");
                        return;
                    }
                }
                else // Базовый продукт уже есть в БД
                {
                    var userProduct = new UserProduct()
                    {
                        UserId = await GetCurrentUserId(),
                        BaseProductId = SelectedBaseItem.Id,
                        CustomNutrition = new NutritionInfo()
                        {
                            Calories = SelectedBaseItem.Calories,
                            Protein = SelectedBaseItem.Protein,
                            Fat = SelectedBaseItem.Fat,
                            Carbs = SelectedBaseItem.Carbs
                        }
                    };

                    createdUserProduct = await _apiService.AddUserProductAsync(userProduct);
                    if (createdUserProduct is null)
                    {
                        _notificationService.ShowError("Ошибка добавления", "Не удалось добавить продукт пользователя");
                        return;
                    }
                }

                var userDishIngredient = new UserDishIngredientViewModel()
                {
                    UserProductId = createdUserProduct.Id,
                    Name = createdUserProduct.BaseProduct?.Name ?? SelectedBaseItem.Name,
                    Quantity = SelectedBaseItem.Quantity,
                    ProductNameSnapshot = SelectedBaseItem.Name!,
                    ProductNutritionInfoSnapshot = new NutritionInfo()
                    {
                        Calories = SelectedBaseItem.Calories * (Quantity / 100),
                        Protein = SelectedBaseItem.Protein * (Quantity / 100),
                        Fat = SelectedBaseItem.Fat * (Quantity / 100),
                        Carbs = SelectedBaseItem.Carbs * (Quantity / 100)
                    },
                    CurrentNutrition = new NutritionInfo()
                    {
                        Calories = SelectedBaseItem.Calories * (Quantity / 100),
                        Protein = SelectedBaseItem.Protein * (Quantity / 100),
                        Fat = SelectedBaseItem.Fat * (Quantity / 100),
                        Carbs = SelectedBaseItem.Carbs * (Quantity / 100)
                    }
                };

                WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(userDishIngredient));                               
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AddUserDishIngredientViewModel]: {ex.Message}");
            }          
        }

        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newProduct = await CreateNewUserItem();

            if (newProduct is not null)
            {
                var userIngredient = new UserDishIngredientViewModel
                {
                    UserProductId = newProduct.Id,
                    Name = ManualName!,
                    Quantity = Quantity,
                    ProductNameSnapshot = ManualName!,
                    ProductNutritionInfoSnapshot = new NutritionInfo()
                    {
                        Calories = ManualCalories * (Quantity / 100),
                        Protein = ManualProtein * (Quantity / 100),
                        Fat = ManualFat * (Quantity / 100),
                        Carbs = ManualCarbs * (Quantity / 100)
                    },
                    CurrentNutrition = new NutritionInfo()
                    {
                        Calories = ManualCalories * (Quantity / 100),
                        Protein = ManualProtein * (Quantity / 100),
                        Fat = ManualFat * (Quantity / 100),
                        Carbs = ManualCarbs * (Quantity / 100)
                    }
                };

                ClearManualEntries();

                WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(userIngredient));
            }
            ClearManualEntries();
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

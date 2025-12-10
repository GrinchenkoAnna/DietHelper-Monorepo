using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;
using DietHelper.ViewModels.Products;

namespace DietHelper.ViewModels.Dishes
{
    public partial class AddDishIngredientViewModel : AddItemBaseViewModel<Product, ProductViewModel>
    {
        public AddDishIngredientViewModel(DatabaseService dbService) : base(dbService)
        {
        }

        protected override async void InitializeMockData()
        {
            var products = await _dbService.GetProductsAsync();

            foreach (var product in products)
            {
                if (product.Id > 0)
                {
                    SearchResults.Add(new ProductViewModel(product));
                    //AllItems.Add(new ProductViewModel(product));
                }
            }
        }

        protected override Product CreateNewItem()
        {
            return new Product()
            {
                Name = ManualName!,
                NutritionFacts = new NutritionInfo()
                {
                    Calories = ManualCalories,
                    Protein = ManualProtein,
                    Fat = ManualFat,
                    Carbs = ManualCarbs
                }
            };
        }

        protected override void AddItem()
        {
            if (SelectedItem is not null)
                WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(SelectedItem));
        }

        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newIngredient = CreateNewItem();
            await _dbService.AddProductAsync(newIngredient);

            var newItem = new ProductViewModel(newIngredient);

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddDishIngredientClosedMessage(newItem));
        }

        protected override async void DeleteItemFromDatabase(ProductViewModel productViewModel)
        {
            SearchResults.Remove(productViewModel);
            //AllItems.Remove(productViewModel);
            await _dbService.DeleteProductAsync(productViewModel.Id);

            WeakReferenceMessenger.Default.Send(new ProductDeleteMessage(productViewModel.Id));
        }
    }
}

using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Products;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;

namespace DietHelper.ViewModels.Products
{
    public partial class AddProductViewModel : AddItemBaseViewModel<Product, ProductViewModel>
    {
        public AddProductViewModel(DatabaseService dbService) : base(dbService)
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

        protected override void AddItem()
        {
            if (SelectedItem is not null)
                WeakReferenceMessenger.Default.Send(new AddProductClosedMessage(SelectedItem));
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

        protected override async void AddManualItem()
        {
            if (string.IsNullOrEmpty(ManualName)) return;

            var newProduct = CreateNewItem();            
            await _dbService.AddProductAsync(newProduct);

            var newItem = new ProductViewModel(newProduct);

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddProductClosedMessage(newItem));
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

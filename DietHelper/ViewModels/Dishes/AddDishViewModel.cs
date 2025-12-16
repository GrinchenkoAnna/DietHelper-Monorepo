using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels.Base;

namespace DietHelper.ViewModels.Dishes
{
    public partial class AddDishViewModel : AddItemBaseViewModel<Dish, DishViewModel>
    {
        private readonly NutritionCalculator _nutritionCalculator;

        public AddDishViewModel(DatabaseService dbService, NutritionCalculator nutritionCalculator) : base(dbService)
        {
            _nutritionCalculator = nutritionCalculator;
        }

        protected override async void InitializeData()
        {
            var dishes = await _dbService.GetDishesAsync();

            foreach (var dish in dishes)
            {
                if (dish.Id > 0)
                {
                    BaseSearchResults.Add(new DishViewModel(dish, _nutritionCalculator));
                    //AllUserItems.Add(new DishViewModel(dish, _calculator));
                }                
            }
        }
        
        protected override void AddUserItem()
        {
            if (SelectedItem is not null)
            {
                WeakReferenceMessenger.Default.Send(new AddDishClosedMessage(SelectedItem));
            }
        }

        private bool IsEmpty()
        {
            return ManualCalories == 0
                && ManualProtein == 0
                && ManualFat == 0
                && ManualCarbs == 0;
        }

        protected override Dish CreateNewUserItem()
        {
            return new Dish()
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

            var newDish = CreateNewUserItem();            

            var newItem = new DishViewModel(newDish, _nutritionCalculator, true);
            if (!IsEmpty()) newItem.Quantity = 100;
            else newItem.Quantity = 0;
            newItem.NutritionFacts = newDish.NutritionFacts;

            newItem.Id = await _dbService.AddDishAsync(newDish);

            ClearManualEntries();

            WeakReferenceMessenger.Default.Send(new AddDishClosedMessage(newItem));
        }

        protected override async void DeleteItemFromDatabase(DishViewModel dishViewModel)
        {
            BaseSearchResults.Remove(dishViewModel);
            //AllUserItems.Remove(dishViewModel);
            await _dbService.DeleteDishAsync(dishViewModel.Id);

            WeakReferenceMessenger.Default.Send(new DeleteUserDishMessage(dishViewModel.Id));
        }
    }
}

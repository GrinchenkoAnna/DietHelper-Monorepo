using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.Core;
using DietHelper.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class StatsViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private int interval = 7;

        [ObservableProperty]
        private DateTime startDay = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime endDay = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<UserMealEntryDto> userMeals = new();

        [ObservableProperty]
        private NutritionInfo totalNutrition = new();

        [ObservableProperty]
        private ObservableCollection<NutritionInfo> nutritions = new();

        public StatsViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task LoadStats()
        {
            UserMeals.Clear();

            var userMealsList = await _apiService.GetUserMealsForPeriod(StartDay, EndDay);
            UserMeals = new ObservableCollection<UserMealEntryDto>(userMealsList ?? new List<UserMealEntryDto>());

            CalculateStats();
        }

        private void CalculateStats()
        {
            Nutritions.Clear();

            var totalNutritions = new NutritionInfo();
            var date = DateTime.Now;

            foreach (var userMeal in UserMeals)
            {
                if (userMeal.Date != date)
                {
                    Nutritions.Add(userMeal.TotalNutrition);
                }
                else
                {
                    Nutritions.Last().Calories += userMeal.TotalNutrition.Calories;
                    Nutritions.Last().Protein += userMeal.TotalNutrition.Protein;
                    Nutritions.Last().Fat += userMeal.TotalNutrition.Fat;
                    Nutritions.Last().Carbs += userMeal.TotalNutrition.Carbs;
                }

                totalNutritions.Calories += userMeal.TotalNutrition.Calories;
                totalNutritions.Protein += userMeal.TotalNutrition.Protein;
                totalNutritions.Fat += userMeal.TotalNutrition.Fat;
                totalNutritions.Carbs += userMeal.TotalNutrition.Carbs;

                date = userMeal.Date;
            }

            TotalNutrition = totalNutritions;
        }
    }
}

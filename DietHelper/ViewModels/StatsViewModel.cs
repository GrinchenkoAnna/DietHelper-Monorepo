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
        private List<NutritionInfo> nutritions = new();

        protected StatsViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task LoadStats()
        {
            //загрузить за период (сначала реализовать в ApiService)
        }

        [RelayCommand]
        private async Task CalculateStats()
        {
            Nutritions.Clear();

            var totalNutritions = new NutritionInfo();

            foreach (var userMeal in UserMeals)
            {
                Nutritions.Add(userMeal.TotalNutrition);

                totalNutritions.Calories += userMeal.TotalNutrition.Calories;
                totalNutritions.Protein += userMeal.TotalNutrition.Protein;
                totalNutritions.Fat += userMeal.TotalNutrition.Fat;
                totalNutritions.Carbs += userMeal.TotalNutrition.Carbs;
            }

            TotalNutrition = totalNutritions;
        }
    }
}

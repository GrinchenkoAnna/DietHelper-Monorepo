using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.Core;
using DietHelper.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        [ObservableProperty]
        private ObservableCollection<ISeries> nutritionSeries = new();

        [ObservableProperty]
        private ObservableCollection<Axis> xAxes = new()
        {
            new Axis
            {
                Name = "Дата"
            }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> yAxes = new()
        {
            new Axis
            {
                Name = "Значения"
            }
        };

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

            var groups = UserMeals
                .GroupBy(um => um.Date)
                .OrderBy(g => g.Key)
                .Select(g => new 
                { 
                    Date = g.Key,
                    Calories = g.Sum(um => um.TotalNutrition.Calories),
                    Protein = g.Sum(um => um.TotalNutrition.Protein),
                    Fat = g.Sum(um => um.TotalNutrition.Fat),
                    Carbs = g.Sum(um => um.TotalNutrition.Carbs)
                })
                .ToList();

            var dateIndices = groups.Select((_, i) => i).ToArray();
            var caloriesValues = groups.Select(g => (double?)g.Calories).ToArray();
            var proteinValues = groups.Select(g => (double?)g.Protein).ToArray();
            var fatValues = groups.Select(g => (double?)g.Fat).ToArray();
            var carbsValues = groups.Select(g => (double?)g.Carbs).ToArray();

            var newSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double?>
                {
                    Name = "Калории",
                    Values = caloriesValues,
                    Fill = new SolidColorPaint(SKColors.Red)
                },
                new ColumnSeries<double?>
                {
                    Name = "Белки",
                    Values = proteinValues,
                    Fill = new SolidColorPaint(SKColors.Blue)
                },
                new ColumnSeries<double?>
                {
                    Name = "Жиры",
                    Values = fatValues,
                    Fill = new SolidColorPaint(SKColors.Orange)
                },
                new ColumnSeries<double?>
                {
                    Name = "Углеводы",
                    Values = carbsValues,
                    Fill = new SolidColorPaint(SKColors.Green)
                }
            };

            XAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = groups.Select(g => g.Date.ToString("dd.MM")).ToList(),
                    LabelsRotation = 15,
                    Name = "Дата"
                }
            };

            NutritionSeries = newSeries;

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

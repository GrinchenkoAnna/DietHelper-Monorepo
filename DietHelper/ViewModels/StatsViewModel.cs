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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class StatsViewModel : ViewModelBase
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private bool isBusy = true;

        private bool isPresetInterval = true;

        private DateTime startDay = DateTime.Today.AddDays(-7);
        public DateTime StartDay
        {
            get => startDay;
            set
            {
                if (startDay == value) return;
                startDay = value;

                if (!EnsureValidOrder()) OnPropertyChanged();

                if (!isPresetInterval && SelectedPeriodIndex != -1)
                {
                    SelectedPeriodIndex = -1;
                    OnPropertyChanged(nameof(SelectedPeriodIndex));
                }

                _ = LoadStatsAsync();
            }
        }

        private DateTime endDay = DateTime.Today;
        public DateTime EndDay
        {
            get => endDay;
            set
            {
                if (endDay == value) return;
                endDay = value;
                if (!EnsureValidOrder()) OnPropertyChanged();

                if (!isPresetInterval && SelectedPeriodIndex != -1)
                {
                    SelectedPeriodIndex = -1;
                    OnPropertyChanged(nameof(SelectedPeriodIndex));
                }

                _ = LoadStatsAsync();
            }
        }

        private bool EnsureValidOrder()
        {
            if (startDay > endDay)
            {
                var temp = startDay;
                startDay = endDay;
                endDay = temp;

                OnPropertyChanged(nameof(StartDay));
                OnPropertyChanged(nameof(EndDay));
                return true;
            }
            return false;
        }

        private int selectedPeriodIndex = 1;
        public int SelectedPeriodIndex
        {
            get => selectedPeriodIndex;
            set
            {
                if (selectedPeriodIndex == value) return;
                selectedPeriodIndex = value;
                UpdatePeriod();
                OnPropertyChanged();
            }
        }

        private void UpdatePeriod()
        {
            isPresetInterval = true;

            try
            {
                var today = DateTime.Today;
                switch (SelectedPeriodIndex)
                {
                    case 0:
                        StartDay = today.AddDays(-3);
                        EndDay = today;
                        break;
                    case 1:
                        StartDay = today.AddDays(-7);
                        EndDay = today;
                        break;
                    case 2:
                        var daysInCurrentMonth = DateTime.DaysInMonth(today.Year, today.Month);
                        StartDay = today.AddDays(-daysInCurrentMonth);
                        EndDay = today;
                        break;
                    default: return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StatsViewModel: {ex.Message}");
            }
            finally
            {
                isPresetInterval = false;
            }
        }

        [ObservableProperty]
        private ObservableCollection<UserMealEntryDto> userMeals = new();

        [ObservableProperty]
        private NutritionInfo totalNutrition = new();

        [ObservableProperty]
        private ObservableCollection<NutritionInfo> nutritions = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> macronutrientsSeries = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> caloriesSeries = new();

        [ObservableProperty]
        private ObservableCollection<Axis> xAxesMacronutrients = new()
        {
            new Axis
            {
                Name = "Дата"
            }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> yAxesMacronutrients = new()
        {
            new Axis
            {
                Name = "грамм"
            }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> xAxesCalories = new()
        {
            new Axis
            {
                Name = "Дата"
            }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> yAxesCalories = new()
        {
            new Axis
            {
                Name = "ккал"
            }
        };

        public StatsViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadStatsAsync()
        {
            IsBusy = true;

            try
            {
                UserMeals.Clear();

                var userMealsList = await _apiService.GetUserMealsForPeriod(StartDay, EndDay);
                UserMeals = new ObservableCollection<UserMealEntryDto>(userMealsList ?? new List<UserMealEntryDto>());

                CalculateStats();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StatsViewModel: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void SetSeries()
        {
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

            var newCaloriesSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double?>
                {
                    Name = "Калории",
                    Values = caloriesValues,
                    Fill = new SolidColorPaint(SKColors.Red)
                }
            };

            XAxesCalories = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = groups.Select(g => g.Date.ToString("dd.MM")).ToList(),
                    Name = "Дата"
                }
            };

            CaloriesSeries = newCaloriesSeries;

            var proteinValues = groups.Select(g => (double?)g.Protein).ToArray();
            var fatValues = groups.Select(g => (double?)g.Fat).ToArray();
            var carbsValues = groups.Select(g => (double?)g.Carbs).ToArray();

            var newMacronutrientsSeries = new ObservableCollection<ISeries>
            {
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

            XAxesMacronutrients = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = groups.Select(g => g.Date.ToString("dd.MM")).ToList(),
                    Name = "Дата"
                }
            };

            MacronutrientsSeries = newMacronutrientsSeries;
        }

        private void CalculateStats()
        {
            Nutritions.Clear();

            SetSeries();

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

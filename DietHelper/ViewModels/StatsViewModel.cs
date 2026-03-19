using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class StatsViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;

        [ObservableProperty] private bool isBusy = true;

        private DateTime startDay = DateTime.Today.AddDays(-7);
        public DateTime StartDay
        {
            get => startDay;
            set
            {
                if (startDay == value) return;
                startDay = value;

                EnsureValidOrder();

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

                EnsureValidOrder();

                _ = LoadStatsAsync();
            }
        }

        private void EnsureValidOrder()
        {
            if (startDay > endDay) (startDay, endDay) = (endDay, startDay);

            OnPropertyChanged(nameof(StartDay));
            OnPropertyChanged(nameof(EndDay));

            int presetIndex = GetPresetIndex();
            if (presetIndex >= 0) SelectedPeriodIndex = presetIndex;
            else SelectedPeriodIndex = -1;
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

        private int GetPresetIndex()
        {
            try
            {
                var today = DateTime.Today;

                if (StartDay == today.AddDays(-3) && EndDay == today) return 0;

                if (StartDay == today.AddDays(-7) && EndDay == today) return 1;

                int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                if (StartDay == today.AddDays(-daysInMonth) && EndDay == today) return 2;

                return -1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StatsViewModel: {ex.Message}");
                return -1;
            }
        }

        private void UpdatePeriod()
        {
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
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StatsViewModel: {ex.Message}");
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
            new Axis { Name = "Дата" }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> yAxesMacronutrients = new()
        {
            new Axis { Name = "грамм" }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> xAxesCalories = new()
        {
            new Axis { Name = "Дата" }
        };

        [ObservableProperty]
        private ObservableCollection<Axis> yAxesCalories = new()
        {
            new Axis { Name = "ккал" }
        };

        public StatsViewModel(IApiService apiService) : base(apiService)
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

        [RelayCommand]
        private async Task ExportStats()
        {
            var mainWindow = App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
            if (mainWindow is null) return;

            var topLevel = TopLevel.GetTopLevel(mainWindow);
            if (topLevel is null) return;

            var options = new FilePickerSaveOptions
            {
                Title = "Экспорт статистики",
                DefaultExtension = "txt",
                SuggestedFileName = $"DietHelper_Статистика_{StartDay:dd-MM-yyyy}-{EndDay:dd-MM-yyyy}.txt",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Текстовые файлы")
                    {
                        Patterns = new[] { "*.txt" },
                    }
                }
            };
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

            if (file is not null)
            {
                await GenerateStatsFileAsync(file.Path.LocalPath);
            }
        }

        private async Task GenerateStatsFileAsync(string filePath)
        {
            var statsSB = new StringBuilder();

            var days = UserMeals.GroupBy(um => um.Date).OrderBy(g => g.Key);
            foreach (var day in days)
            {
                statsSB.AppendLine($"Дата: {day.Key:ddd dd.MM.yyyy}");

                foreach (var meal in day)
                {
                    string mealName = meal.UserDishName ?? meal.Ingredients?.FirstOrDefault()?.ProductNameSnapshot ?? "Без названия";
                    statsSB.AppendLine($"- {mealName}:\n" +
                        $"  К: {meal.TotalNutrition.FormattedCalories,-12}   " +
                        $"Б: {meal.TotalNutrition.FormattedProtein,-10}   " +
                        $"Ж: {meal.TotalNutrition.FormattedFat,-10}   " +
                        $"У: {meal.TotalNutrition.FormattedCarbs,-10}   " +
                        $"вес: {meal.TotalQuantity,-5:F0} г");
                }
                statsSB.AppendLine();
            }

            statsSB.AppendLine($"Итого:\n" +
                $"  К: {TotalNutrition.FormattedCalories,-12}   " +
                $"Б: {TotalNutrition.FormattedProtein,-10}   " +
                $"Ж: {TotalNutrition.FormattedFat,-10}   " +
                $"У: {TotalNutrition.FormattedCarbs,-10}");

            await File.WriteAllTextAsync(filePath, statsSB.ToString());
        }
    }
}

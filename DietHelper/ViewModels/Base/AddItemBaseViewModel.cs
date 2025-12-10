using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DietHelper.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DietHelper.ViewModels.Base
{
    public abstract partial class AddItemBaseViewModel<TModel, TViewModel> : ViewModelBase, IAddItemViewModel
        where TModel : class
        where TViewModel : class
    {
        protected readonly DatabaseService _dbService;

        [ObservableProperty] private string? searchText = string.Empty;
        [ObservableProperty] private bool isBusy = false;

        [ObservableProperty] private TViewModel? selectedItem;
        
        public ObservableCollection<TViewModel> SearchResults { get; } = new();

        // заменить на подгрузку из бд или вообще переделать коллекции
        protected ObservableCollection<TViewModel> AllItems { get; } = new();

        [ObservableProperty] private string? manualName;
        [ObservableProperty] private double manualCalories;
        [ObservableProperty] private double manualProtein;
        [ObservableProperty] private double manualFat;
        [ObservableProperty] private double manualCarbs;
        protected abstract TModel CreateNewItem();
        protected void ClearManualEntries()
        {
            ManualName = string.Empty;
            ManualCalories = 0;
            ManualProtein = 0;
            ManualFat = 0;
            ManualCarbs = 0;
        }

        protected AddItemBaseViewModel(DatabaseService dbService)
        {
            _dbService = dbService;
            InitializeMockData();
        }

        protected abstract void InitializeMockData();

        partial void OnSearchTextChanged(string? value)
        {
            _ = DoSearch(SearchText);
        }

        private async Task DoSearch(string? term)
        {
            IsBusy = true;
            SearchResults.Clear();

            // временно для имитации работы
            foreach (var item in AllItems)
            {
                // не очень эффективный алгоритм поиска
                if (term is not null && item.GetType().GetProperty("Name")!.GetValue(item)!.ToString()
                    .Contains(term, System.StringComparison.CurrentCultureIgnoreCase))
                    SearchResults.Add(item);
            }

            IsBusy = false;
        }

        [RelayCommand]
        protected abstract void AddItem();

        [RelayCommand]
        protected abstract void AddManualItem();

        [RelayCommand]
        protected abstract void DeleteItemFromDatabase(TViewModel viewModel);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DietHelper.ViewModels;
using HarfBuzzSharp;

namespace DietHelper.Views;

public partial class StatsWindow : Window
{
    public StatsWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (DataContext is StatsViewModel viewModel)
                await viewModel.LoadStatsAsync();
        };
    }
}
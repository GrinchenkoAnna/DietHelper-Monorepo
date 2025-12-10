using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;
using DietHelper.ViewModels;
using DietHelper.ViewModels.Dishes;
using DietHelper.ViewModels.Products;
using DietHelper.Views.Dishes;
using DietHelper.Views.Products;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace DietHelper.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<MainWindow, AddProductMessage>(this, static (w, m) =>
            {
                var viewModel = w._serviceProvider.GetRequiredService<AddProductViewModel>();
                
                var dialog = new AddProductWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<ProductViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddDishMessage>(this, static (w, m) =>
            {
                var viewModel = w._serviceProvider.GetRequiredService<AddDishViewModel>();

                var dialog = new AddDishWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<DishViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddDishIngredientMessage>(this, static (w, m) =>
            {
                var viewModel = w._serviceProvider.GetRequiredService<AddProductViewModel>();

                var dialog = new AddDishIngredientWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<ProductViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, ProductDeleteMessage>(this, static (w, m) =>
            {
                if (w.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    var productToRemove = mainWindowViewModel.Products.FirstOrDefault(p => p.Id == m.Value);

                    if (productToRemove is not null) 
                        mainWindowViewModel.Products.Remove(productToRemove);

                    foreach (var dish in mainWindowViewModel.Dishes)
                    {
                        var ingredientToRemove = dish.Ingredients.FirstOrDefault(di => di.Id == m.Value);

                        if (ingredientToRemove is not null)
                            dish.Ingredients.Remove(ingredientToRemove);
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<MainWindow, DishDeleteMessage>(this, static (w, m) =>
            {
                if (w.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    var dishToRemove = mainWindowViewModel.Dishes.FirstOrDefault(p => p.Id == m.Value);

                    if (dishToRemove is not null) 
                        mainWindowViewModel.Dishes.Remove(dishToRemove);
                }
            });
        }
    }
}
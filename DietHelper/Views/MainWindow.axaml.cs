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

            WeakReferenceMessenger.Default.Register<MainWindow, AddBaseProductMessage>(this, static (w, m) =>
            {
                var viewModel = ServiceLocator.GetRequiredService<AddProductViewModel>();

                var dialog = new AddProductWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<BaseProductViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddUserProductMessage>(this, static (w, m) =>
            {
                var viewModel = ServiceLocator.GetRequiredService<AddProductViewModel>();
                
                var dialog = new AddProductWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<UserProductViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddUserDishMessage>(this, static (w, m) =>
            {
                var viewModel = ServiceLocator.GetRequiredService<AddUserDishViewModel>();

                var dialog = new AddDishWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<UserDishViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddDishIngredientMessage>(this, static (w, m) =>
            {
                var viewModel = ServiceLocator.GetRequiredService<AddProductViewModel>(); //разделить продукты и ингредиенты

                var dialog = new AddDishIngredientWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<UserDishIngredientViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, DeleteUserProductMessage>(this, static (w, m) =>
            {
                if (w.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    var userProductToRemove = mainWindowViewModel.UserProducts.FirstOrDefault(p => p.Id == m.Value);

                    if (userProductToRemove is not null) 
                        mainWindowViewModel.UserProducts.Remove(userProductToRemove);

                    foreach (var userDish in mainWindowViewModel.UserDishes)
                    {
                        var ingredientToRemove = userDish.Ingredients.FirstOrDefault(i => i.Id == m.Value);

                        if (ingredientToRemove is not null)
                            userDish.Ingredients.Remove(ingredientToRemove);
                    }
                }
            });

            WeakReferenceMessenger.Default.Register<MainWindow, DeleteUserDishMessage>(this, static (w, m) =>
            {
                if (w.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    var userDishToRemove = mainWindowViewModel.UserDishes.FirstOrDefault(d => d.Id == m.Value);

                    if (userDishToRemove is not null) 
                        mainWindowViewModel.UserDishes.Remove(userDishToRemove);
                }
            });
        }
    }
}
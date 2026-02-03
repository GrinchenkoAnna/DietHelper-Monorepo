using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;
using DietHelper.Services;
using DietHelper.ViewModels;
using DietHelper.ViewModels.Dishes;
using DietHelper.ViewModels.Products;
using DietHelper.Views.Dishes;
using DietHelper.Views.Products;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DietHelper.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private readonly ApiService _apiService;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _navigationService = serviceProvider.GetRequiredService<INavigationService>();
            _apiService = serviceProvider.GetRequiredService<ApiService>();

            _apiService.AuthStateChanged += OnAuthStateChanged;
            _navigationService.NavigationRequested += OnNavigationRequested;

            NavigateBaseOnAuthState();
        }

        private void OnAuthStateChanged()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                NavigateBaseOnAuthState();
            });
        }

        private void NavigateBaseOnAuthState()
        {
            if (_apiService.IsAuthenticated) _navigationService.NavigateToMainAsync();
            else _navigationService.NavigateToLoginAsync();            
        }

        private void OnNavigationRequested(object? sender, Type viewModel)
        {
            if (viewModel == typeof(AuthViewModel))
            {
                var authView = new AuthView()
                {
                    DataContext = _serviceProvider.GetRequiredService<AuthViewModel>()
                };
                MainContent = authView;

                WeakReferenceMessenger.Default.UnregisterAll(this);
            }
            else if (viewModel == typeof(MainWindowViewModel))
            {
                var mainView = new MainView()
                {
                    DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
                };
                MainContent = mainView;

                SetupMessaging();
            }
        }        

        //подписки на сообщения
        private void SetupMessaging()
        {
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
                var viewModel = ServiceLocator.GetRequiredService<AddUserDishIngredientViewModel>();

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
                        var ingredientToRemove = userDish.Ingredients.FirstOrDefault(i => i.UserProductId == m.Value);

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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _apiService.AuthStateChanged -= OnAuthStateChanged;
            _navigationService.NavigationRequested -= OnNavigationRequested;

            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
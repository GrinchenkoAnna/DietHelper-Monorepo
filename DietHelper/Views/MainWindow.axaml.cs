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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DietHelper.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApiService _apiService;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _serviceProvider = serviceProvider;
            _apiService = serviceProvider.GetRequiredService<ApiService>();

            Debug.WriteLine($"[MainWindow] Constructor called. ApiService hash: {_apiService.GetHashCode()}");

            _apiService.AuthStateChanged += OnAuthStateChanged;

            NavigateBaseOnAuthState();
        }

        private async void OnAuthStateChanged()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    try
                    {
                        NavigateBaseOnAuthState();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[MainWindow] ERROR in NavigateBaseOnAuthState: {ex.Message}");
                    }                    
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] ERROR in OnAuthStateChanged: {ex.Message}");
            }            
        }

        private void NavigateBaseOnAuthState()
        {
            try
            {
                var currentContentType = MainContent.Content?.GetType().Name ?? "null";

                if (_apiService.IsAuthenticated)
                {
                    if (currentContentType == "MainView") return;

                    var mainView = new MainView();
                    var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

                    if (viewModel == null) return;

                    mainView.DataContext = viewModel;
                    MainContent.Content = mainView;

                    SetupMessaging();
                }
                else
                {
                    MainContent.Content = null;

                    if (currentContentType == "AuthView") return;

                    var authView = new AuthView();
                    var viewModel = _serviceProvider.GetRequiredService<AuthViewModel>();

                    if (viewModel == null) return;

                    authView.DataContext = viewModel;
                    MainContent.Content = authView;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var authView = new AuthView();
                    var viewModel = _serviceProvider.GetRequiredService<AuthViewModel>();
                    authView.DataContext = viewModel;
                    MainContent.Content = authView;
                }
                catch (Exception inEx)
                {
                    Debug.WriteLine($"[MainWindow]: {inEx.Message}");
                }
            }
        }

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

            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}
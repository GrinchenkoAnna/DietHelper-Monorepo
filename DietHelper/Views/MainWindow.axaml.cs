using Avalonia.Controls;
using Avalonia.Controls.Notifications;
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
using System.Diagnostics;

namespace DietHelper.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApiService _apiService;
        public static WindowNotificationManager _notificationManager;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _notificationManager = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3
            };

            _serviceProvider = serviceProvider;
            _apiService = serviceProvider.GetRequiredService<IApiService>();

            Debug.WriteLine($"[MainWindow] Constructor called. ApiService hash: {_apiService.GetHashCode()}");

            _apiService.AuthStateChanged -= OnAuthStateChanged;
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

                    WeakReferenceMessenger.Default.UnregisterAll(this);
                    MainContent.Content = null;

                    var mainView = new MainView();
                    var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

                    if (viewModel == null) return;

                    mainView.DataContext = viewModel;
                    MainContent.Content = mainView;

                    SetupMessaging();
                }
                else
                {
                    if (currentContentType == "AuthView") return;
                    MainContent.Content = null;

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
                 var viewModel = w._serviceProvider.GetRequiredService<AddProductViewModel>();

                 var dialog = new AddProductWindow
                 {
                     DataContext = viewModel
                 };
                 m.Reply(dialog.ShowDialog<BaseProductViewModel?>(w));
             });

            WeakReferenceMessenger.Default.Register<MainWindow, AddUserProductMessage>(this, static (w, m) =>
            {
                var viewModel = w._serviceProvider.GetRequiredService<AddProductViewModel>();

                var dialog = new AddProductWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<UserProductViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddUserDishMessage>(this, static (w, m) =>
            {
                var viewModel = w._serviceProvider.GetRequiredService<AddUserDishViewModel>();

                var dialog = new AddDishWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<UserDishViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<MainWindow, AddDishIngredientMessage>(this, static (w, m) =>
            {
                var viewModel = w._serviceProvider.GetRequiredService<AddUserDishIngredientViewModel>();

                var dialog = new AddDishIngredientWindow
                {
                    DataContext = viewModel
                };
                m.Reply(dialog.ShowDialog<UserDishIngredientViewModel?>(w));
            });

            WeakReferenceMessenger.Default.Register<NotificationMessages>(this, (w, m) =>
            {
                if (this.IsActive)
                {
                    _notificationManager?.Show(new Notification
                    {
                        Title = m.Title,
                        Message = m.Message,
                        Type = m.Type
                    });
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
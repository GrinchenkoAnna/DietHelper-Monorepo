using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DietHelper.Services;
using DietHelper.ViewModels;
using DietHelper.ViewModels.Base;
using DietHelper.ViewModels.Dishes;
using DietHelper.ViewModels.Products;
using DietHelper.Views;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;

namespace DietHelper
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

            var services = new ServiceCollection();

            services.AddSingleton<NutritionCalculator>();
            services.AddSingleton(sp =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7206/api/")
                };
                return client;
            });
            services.AddSingleton<ApiService>();
            //services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<ViewModelBase>();
            services.AddTransient<UserDishViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<AddProductViewModel>();
            services.AddTransient<AddUserDishViewModel>();
            services.AddTransient<AddUserDishIngredientViewModel>();
            services.AddTransient<AuthViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            ServiceLocator.Initialize(_serviceProvider);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                //var navigationService = _serviceProvider.GetRequiredService<INavigationService>() as NavigationService;

                var mainWindow = new MainWindow(_serviceProvider);
                desktop.MainWindow = mainWindow;

                if (desktop.MainWindow is not null && desktop.MainWindow != mainWindow)
                {
                    desktop.MainWindow.Close();
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
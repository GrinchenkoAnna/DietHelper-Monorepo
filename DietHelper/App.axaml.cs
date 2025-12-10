using System;
using System.Linq;
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
using Microsoft.Extensions.DependencyInjection;

namespace DietHelper
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();

            services.AddSingleton<NutritionCalculator>();
            //services.AddSingleton<DatabaseService>();
            services.AddSingleton<ApiService>();

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<AddProductViewModel>();
            services.AddTransient<AddDishViewModel>();
            services.AddTransient<AddDishIngredientViewModel>();

            _serviceProvider = services.BuildServiceProvider();


            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
                };
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
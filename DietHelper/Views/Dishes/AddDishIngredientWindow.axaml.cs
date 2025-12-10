using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;

namespace DietHelper.Views.Dishes;

public partial class AddDishIngredientWindow : Window
{
    public AddDishIngredientWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<AddDishIngredientWindow, AddDishIngredientClosedMessage>(this, static (window, message) =>
        {
            window.Close(message.SelectedIngredient);
        });
    }
}
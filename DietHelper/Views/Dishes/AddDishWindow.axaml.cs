using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;

namespace DietHelper.Views.Dishes;

public partial class AddDishWindow : Window
{
    public AddDishWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<AddDishWindow, AddDishClosedMessage>(this, static (window, message) =>
            {
                window.Close(message.SelectedDish);
            });
    }
}
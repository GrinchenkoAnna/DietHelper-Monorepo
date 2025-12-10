using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;

namespace DietHelper.Views.Products;

public partial class AddProductWindow : Window
{
    public AddProductWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<AddProductWindow, AddProductClosedMessage>(this,
            static (window, message) =>
            {
                window.Close(message.SelectedProduct);
            });
    }
}
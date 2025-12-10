using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;
using DietHelper.ViewModels.Dishes;

namespace DietHelper.Views.Dishes;

public partial class DishView : UserControl
{
    public DishView()
    {
        InitializeComponent();
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DietHelper.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void Button_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}
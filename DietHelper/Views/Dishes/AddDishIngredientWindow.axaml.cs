using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;

namespace DietHelper.Views.Dishes;

public partial class AddDishIngredientWindow : Window
{
    private WindowNotificationManager _notificationManager;

    public AddDishIngredientWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<AddDishIngredientWindow, AddDishIngredientClosedMessage>(this, static (window, message) =>
        {
            window.Close(message.SelectedIngredient);
        });

        _notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 3
        };

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
}
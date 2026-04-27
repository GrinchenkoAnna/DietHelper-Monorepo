using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;

namespace DietHelper.Views.Products;

public partial class AddProductWindow : Window
{
    private WindowNotificationManager _notificationManager;

    public AddProductWindow()
    {
        InitializeComponent();

        WeakReferenceMessenger.Default.Register<AddProductWindow, AddUserProductClosedMessage>(this,
            static (window, message) =>
            {
                window.Close(message.SelectedProduct);
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
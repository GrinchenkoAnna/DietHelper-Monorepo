using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;
using DietHelper.ViewModels;
using HarfBuzzSharp;

namespace DietHelper.Views;

public partial class StatsWindow : Window
{
    private WindowNotificationManager _notificationManager;
    public StatsWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (DataContext is StatsViewModel viewModel)
                await viewModel.LoadStatsAsync();
        };

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
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Models.Messages;
using DietHelper.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DietHelper.Services
{
    public interface INotificationService
    {
        void ShowError(string title, string message);
        void ShowSuccess(string title, string message);
        void ShowInfo(string title, string message);
    }

    public class NotificationService : INotificationService
    {
        public void ShowError(string title, string message)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessages
            {
                Title = title,
                Message = message,
                Type = NotificationType.Error
            });
        }

        public void ShowInfo(string title, string message)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessages
            {
                Title = title,
                Message = message,
                Type = NotificationType.Information
            });
        }

        public void ShowSuccess(string title, string message)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessages
            {
                Title = title,
                Message = message,
                Type = NotificationType.Success
            });
        }
    }
}

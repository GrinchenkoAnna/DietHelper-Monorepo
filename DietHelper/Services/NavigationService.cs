using DietHelper.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public class NavigationService : INavigationService
    {
        public event EventHandler<Type>? NavigationRequested;

        public Task NavigateToLoginAsync()
        {
            NavigationRequested?.Invoke(this, typeof(AuthViewModel));
            return Task.CompletedTask;
        }

        public Task NavigateToMainAsync()
        {
            NavigationRequested?.Invoke(this, typeof(MainWindowViewModel));
            return Task.CompletedTask;
        }

        public Task NavigateToRegisterAsync()
        {
            // временно
            return NavigateToLoginAsync();
        }
    }
}

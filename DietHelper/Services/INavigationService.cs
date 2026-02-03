using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public interface INavigationService
    {
        public event EventHandler<Type>? NavigationRequested;

        Task NavigateToLoginAsync();

        Task NavigateToMainAsync();

        Task NavigateToRegisterAsync();
    }
}

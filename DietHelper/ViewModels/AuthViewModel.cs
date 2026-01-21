using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly INavigationService _navigationService;

        public AuthViewModel(ApiService apiService, INavigationService navigationService)
        {
            _apiService = apiService;
            _navigationService = navigationService;

            
        }
    }
}

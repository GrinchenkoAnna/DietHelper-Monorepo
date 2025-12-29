using CommunityToolkit.Mvvm.ComponentModel;
using DietHelper.Common.Models;
using DietHelper.Services;
using System.Threading.Tasks;

namespace DietHelper.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        private readonly ApiService _apiService;
        protected ViewModelBase(ApiService apiService)
        {
            _apiService = apiService;
        }

        protected virtual int GetCurrentUserId()
        {
            //временная заглушка
            return 1;
        }

        protected virtual async Task<User> GetCurrentUser()
        {
            //return await _apiService.GetCurrentUser();
            //временная заглушка
            return new User() 
            { 
                Id = GetCurrentUserId(), 
                Name = "System User" 
            };
        }
    }
}

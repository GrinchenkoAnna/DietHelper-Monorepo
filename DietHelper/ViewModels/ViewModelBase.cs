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

        protected virtual async Task<int> GetCurrentUserId()
        {
            var user = await _apiService.GetUserAsync();
            return user.Id;
        }

        protected virtual async Task<User> GetCurrentUser()
        {
            return await _apiService.GetUserAsync();
        }
    }
}

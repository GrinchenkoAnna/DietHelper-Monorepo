using DietHelper.Common.DTO;
using DietHelper.Common.Models.MealEntries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        public async Task<List<UserMealEntry>?> GetUserMealsForDate(DateTime date)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"meals?date={date:yyyy-MM-dd}"));
                var userMeals = await response!.Content.ReadFromJsonAsync<List<UserMealEntry>>();

                return userMeals;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw;
            }
        }

        public async Task<UserMealEntry?> AddUserMealEntryAsync(UserMealEntryDto newUserMealEntry)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateUserMealEntry(int userMealEntryId, UserMealEntryDto userMealEntryDto)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUserMealEntryAsync(int userMealEntryId)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw;
            }
        }
    }
}

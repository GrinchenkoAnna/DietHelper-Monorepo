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

        public async Task<UserMealEntry?> AddUserMealEntryAsync(UserMealEntryDto request)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.PostAsJsonAsync("meals", request), false);
                return await response!.Content.ReadFromJsonAsync<UserMealEntry>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUserMealEntry(int userMealEntryId, UserMealEntryDto request)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.PutAsJsonAsync($"meals/{userMealEntryId}", request), false);
                return response!.IsSuccessStatusCode == true;
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
                var response = await SendRequestAsync(() => _httpClient.DeleteAsync($"meals/{userMealEntryId}"), false);
                return response!.IsSuccessStatusCode == true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw;
            }
        }
    }
}

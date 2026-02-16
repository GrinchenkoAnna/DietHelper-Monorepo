using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        public async Task<List<UserDish>?> GetUserDishesAsync()
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"dishes"), true);
                var userDishes = await response.Content.ReadFromJsonAsync<List<UserDish>>();

                return userDishes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }            
        }

        public async Task<UserDish?> GetUserDishAsync(int userDishId)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"dishes/{userDishId}"), true);
                var UserDish = await response.Content.ReadFromJsonAsync<UserDish>();

                return UserDish;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }            
        }

        public async Task<UserDish?> AddUserDishAsync(UserDish newUserDish)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var json = JsonSerializer.Serialize(newUserDish, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await SendRequestAsync(() => _httpClient.PostAsync("dishes", content), false);
                return await response!.Content.ReadFromJsonAsync<UserDish>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }
        }

        public async Task DeleteDishAsync(int id)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                await SendRequestAsync(() => _httpClient.DeleteAsync($"dishes/{id}"), false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }
        }

        public async Task<int?> AddUserDishIngredientAsync(int dishId, UserDishIngredient userDishIngredient)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var request = new
            {
                UserProductId = userDishIngredient.UserProductId,
                Quantity = userDishIngredient.Quantity
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await SendRequestAsync(() => _httpClient.PostAsync($"dishes/{dishId}/ingredients", content), false);

                var responseContent = await response!.Content.ReadAsStringAsync();
                int.TryParse(responseContent, out int ingredientId);
                return ingredientId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }            
        }

        public async Task<bool> RemoveUserDishIngredientAsync(int dishId, int ingredientId)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.DeleteAsync($"dishes/{dishId}/ingredients/{ingredientId}"), false);
                return response!.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }            
        }
    }
}

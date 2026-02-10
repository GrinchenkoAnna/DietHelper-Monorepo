using DietHelper.Common.Models.Dishes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

            var response = await _httpClient.GetAsync($"dishes");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.GetAsync($"dishes");
            }
            else return null;

            var userDishes = await response.Content.ReadFromJsonAsync<List<UserDish>>();

            return userDishes;
        }

        public async Task<UserDish?> GetUserDishAsync(int id)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var response = await _httpClient.GetAsync($"dishes/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.GetAsync($"dishes/{id}");
            }
            else return null;

            var UserDish = await response.Content.ReadFromJsonAsync<UserDish>();

            return UserDish;
        }

        public async Task<UserDish?> AddUserDishAsync(UserDish newUserDish)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var json = JsonSerializer.Serialize(newUserDish, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("dishes", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.PostAsync("dishes", content);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Server error response: {errorContent}");
                Debug.WriteLine($"Status: {response.StatusCode}");

                throw new HttpRequestException(
                    $"Server returned {response.StatusCode}: {errorContent}",
                    null, response.StatusCode);
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserDish>();
        }

        public async Task DeleteDishAsync(int id)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var response = await _httpClient.DeleteAsync($"dishes/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.DeleteAsync($"dishes/{id}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Server error response: {errorContent}");
                Debug.WriteLine($"Status: {response.StatusCode}");

                throw new HttpRequestException(
                    $"Server returned {response.StatusCode}: {errorContent}",
                    null, response.StatusCode);
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

            var response = await _httpClient.PostAsync(
                $"dishes/{dishId}/ingredients",
                content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.PostAsync($"dishes/{dishId}/ingredients",
                                                            content);
            }

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                if (int.TryParse(responseContent, out var ingredientId))
                    return ingredientId;
            }
            return null;
        }

        public async Task<bool> RemoveUserDishIngredientAsync(int dishId, int ingredientId)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var response = await _httpClient.DeleteAsync($"dishes/{dishId}/ingredients/{ingredientId}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.DeleteAsync($"dishes/{dishId}/ingredients/{ingredientId}");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Server error response: {errorContent}");
                Debug.WriteLine($"Status: {response.StatusCode}");

                throw new HttpRequestException(
                    $"Server returned {response.StatusCode}: {errorContent}",
                    null, response.StatusCode);
            }

            return response.IsSuccessStatusCode;
        }
    }
}

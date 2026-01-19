using DietHelper.Common.Models;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using DietHelper.ViewModels.Dishes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly int _currentUserId; 

        public ApiService() 
        {
            _currentUserId = 1; //временно

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5119/api/")
            };
        }
        
        public async Task<bool> CheckServerConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> GetUserAsync()
        {
            return new User() //заглушка
            {
                Id = _currentUserId
            };
        }

        #region Products
        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"products");

                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();

                var userProducts = await response.Content.ReadFromJsonAsync<List<UserProduct>>();

                return userProducts;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BaseProduct>?> GetBaseProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/base");

                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();

                var baseProducts = await response.Content.ReadFromJsonAsync<List<BaseProduct>>();

                return baseProducts;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }

        public async Task<UserProduct> GetUserProductAsync(int userProductId)
        {
            var response = await _httpClient.GetAsync($"products/{userProductId}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var userProduct = await response.Content.ReadFromJsonAsync<UserProduct>();

            return userProduct;
        }

        public async Task<UserProduct> AddUserProductAsync(UserProduct newUserProduct)
        {
            var json = JsonSerializer.Serialize(newUserProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("products/user", content);

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

            return await response.Content.ReadFromJsonAsync<UserProduct>();
        }

        public async Task<BaseProduct> AddProductAsync<BaseProduct>(BaseProduct newBaseProduct)
        {
            var json = JsonSerializer.Serialize(newBaseProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("products/base", content);

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

            return await response.Content.ReadFromJsonAsync<BaseProduct>();
        }        

        public async Task DeleteUserProductAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"products/{id}");

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
        #endregion

        #region Dishes
        public async Task<List<UserDish>> GetUserDishesAsync()
        {
            var response = await _httpClient.GetAsync($"dishes");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var UserDishes = await response.Content.ReadFromJsonAsync<List<UserDish>>();

            return UserDishes;
        }

        public async Task<UserDish> GetUserDishAsync(int id)
        {
            var response = await _httpClient.GetAsync($"dishes/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var UserDish = await response.Content.ReadFromJsonAsync<UserDish>();

            return UserDish;
        }

        public async Task<UserDish> AddUserDishAsync(UserDish newUserDish)
        {
            var json = JsonSerializer.Serialize(newUserDish, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"dishes", content);

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
            var response = await _httpClient.DeleteAsync($"dishes/{id}");

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
            var response = await _httpClient.DeleteAsync($"dishes/{dishId}/ingredients/{ingredientId}");

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
        #endregion
    }
}

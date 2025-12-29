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
            //throw new NotImplementedException();

            return new User();
        }

        #region Products
        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"products/{_currentUserId}");

                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();

                var userProducts = await response.Content.ReadFromJsonAsync<List<UserProduct>>();

                return userProducts.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }

        public async Task<List<BaseProduct>> GetBaseProductsAsync()
        {
            //throw new NotImplementedException();

            return new List<BaseProduct>();
        }

        public async Task<UserProduct> GetUserProductAsync(int userProductId)
        {
            var response = await _httpClient.GetAsync($"products/{_currentUserId}/{userProductId}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var userProduct = await response.Content.ReadFromJsonAsync<UserProduct>();

            return userProduct;
        }

        public async Task<UserProduct> AddUserProductAsync(UserProduct newUserProduct)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseProduct> AddProductAsync<BaseProduct>(BaseProduct baseProduct)
        {
            throw new NotImplementedException();
        }        

        public async Task DeleteUserProductAsync(int id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Dishes
        public async Task<List<UserDish>> GetUserDishesAsync()
        {
            var response = await _httpClient.GetAsync($"dishes/{_currentUserId}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var UserDishes = await response.Content.ReadFromJsonAsync<List<UserDish>>();

            return UserDishes;
        }

        public async Task<UserDish> GetUserDishAsync(int id)
        {
            var response = await _httpClient.GetAsync($"dishes/{_currentUserId}/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var UserDish = await response.Content.ReadFromJsonAsync<UserDish>();

            return UserDish;
        }

        public async Task<UserDish> AddUserDishAsync(UserDish userDish)
        {
            throw new NotImplementedException();
        }

        //public async Task UpdateUserDishAsync(UserDish userDish)
        //{
        //    var response = await _httpClient.PutAsync($"dishes/{_currentUserId}/{userDish}");

        //    response.EnsureSuccessStatusCode();
        //}

        public async Task DeleteDishAsync(int id)
        {
            throw new NotImplementedException();
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
                $"dishes/{_currentUserId}/{dishId}/ingredients",
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
            var response = await _httpClient.DeleteAsync($"dishes/{_currentUserId}/{dishId}/ingredients/{ingredientId}");
            
            return response.IsSuccessStatusCode;
        }
        #endregion
    }
}

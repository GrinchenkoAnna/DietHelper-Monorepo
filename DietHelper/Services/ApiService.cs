using DietHelper.Common.Models;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

        //public async Task<UserProduct?> GetUserProductsMockAsync()
        //{
        //    var response = await _httpClient.GetAsync("simpleproducts/mock");

        //    if (response.StatusCode == HttpStatusCode.NotFound) return null;

        //    response.EnsureSuccessStatusCode();

        //    return await response.Content.ReadFromJsonAsync<UserProduct>();
        //}

        //public async Task<UserProduct?> GetUserProductMockAsync(int id)
        //{
        //    Debug.WriteLine($"******Запрос продукта с id={id}");
        //    var response = await _httpClient.GetAsync("simpleproducts/mockProduct/{id}");
        //    Debug.WriteLine($"******Статус: {response.StatusCode}");

        //    if (response.StatusCode == HttpStatusCode.NotFound) return null;

        //    response.EnsureSuccessStatusCode();

        //    return await response.Content.ReadFromJsonAsync<UserProduct>();
        //}

        //public async Task<UserDish?> GetUserDishMockAsync(int id)
        //{
        //    var response = await _httpClient.GetAsync("simpleproducts/mockDish/{id}");

        //    if (response.StatusCode == HttpStatusCode.NotFound) return null;

        //    response.EnsureSuccessStatusCode();

        //    return await response.Content.ReadFromJsonAsync<UserDish>();
        //}

        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            try
            {
                Debug.WriteLine($"[ApiService] === ВХОД В GetUserProductsAsync ===");
                Debug.WriteLine($"[ApiService] _currentUserId = {_currentUserId}");
                Debug.WriteLine($"[ApiService] _httpClient.BaseAddress = {_httpClient?.BaseAddress}");
                Debug.WriteLine($"[ApiService] _httpClient is null = {_httpClient == null}");

                // Точка 1 - до GetAsync
                Debug.WriteLine($"[ApiService] Точка 1: Перед GetAsync");

                var response = await _httpClient.GetAsync($"simpleproducts/{_currentUserId}/products");

                // Точка 2 - после GetAsync
                Debug.WriteLine($"[ApiService] Точка 2: После GetAsync, StatusCode = {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();

                var userProducts = await response.Content.ReadFromJsonAsync<List<UserProduct>>();

                Debug.WriteLine($"[ApiService] Точка 3: Десериализовано {userProducts?.Count ?? 0} продуктов");

                return userProducts.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
            finally
            {
                Debug.WriteLine($"[ApiService] === ВЫХОД ИЗ GetUserProductsAsync ===");
            }
        }

        public async Task<List<BaseProduct>> GetBaseProductsAsync()
        {
            //throw new NotImplementedException();

            return new List<BaseProduct>();
        }

        public async Task<UserProduct> GetUserProductAsync(int userProductId)
        {
            //throw new NotImplementedException();

            return new UserProduct();
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

        public async Task<List<UserDish>> GetDishesAsync()
        {
            //throw new NotImplementedException();

            return new List<UserDish>();
        }

        public async Task<UserDish> AddUserDishAsync(UserDish userDish)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserDishAsync(UserDish userDish)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteDishAsync(int id)
        {
            throw new NotImplementedException();
        }        
    }
}

using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"products"), true);
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
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"products/base"), true);
                var baseProducts = await response.Content.ReadFromJsonAsync<List<BaseProduct>>();

                return baseProducts;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }

        public async Task<UserProduct?> GetUserProductAsync(int userProductId)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"products/{userProductId}"), true);
                var userProduct = await response.Content.ReadFromJsonAsync<UserProduct>();

                return userProduct;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }

        public async Task<UserProduct> AddUserProductAsync(UserProduct newUserProduct)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var json = JsonSerializer.Serialize(newUserProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await SendRequestAsync(() => _httpClient.PostAsync($"products/user", content), false);
                return await response!.Content.ReadFromJsonAsync<UserProduct>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }
        }

        public async Task<BaseProduct> AddProductAsync<BaseProduct>(BaseProduct newBaseProduct)
        {
            if (!IsAuthenticated) LoadSavedSession();

            var json = JsonSerializer.Serialize(newBaseProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await SendRequestAsync(() => _httpClient.PostAsync("products/base", content), false);
                return await response!.Content.ReadFromJsonAsync<BaseProduct>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }
        }

        public async Task DeleteUserProductAsync(int id)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                await SendRequestAsync(() => _httpClient.DeleteAsync($"products/{id}"), false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                throw; // обработать далее?
            }
        }
    }
}

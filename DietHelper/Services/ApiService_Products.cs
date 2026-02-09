using DietHelper.Common.Models.Products;
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
        #region Products
        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            if (!IsAuthenticated) return null;

            try
            {
                var response = await _httpClient.GetAsync($"products");

                if (response.StatusCode == HttpStatusCode.NotFound) return null;
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (await RefreshTokensAsync())
                        response = await _httpClient.GetAsync($"products");
                }
                else return null;

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
            if (!IsAuthenticated) return null;

            try
            {
                var response = await _httpClient.GetAsync($"products/base");

                if (response.StatusCode == HttpStatusCode.NotFound) return null;
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (await RefreshTokensAsync())
                        response = await _httpClient.GetAsync($"products/base");
                }
                else return null;

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
            if (!IsAuthenticated) return null;

            var response = await _httpClient.GetAsync($"products/{userProductId}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.GetAsync($"products/{userProductId}");
            }
            else return null;

            var userProduct = await response.Content.ReadFromJsonAsync<UserProduct>();

            return userProduct;
        }

        public async Task<UserProduct> AddUserProductAsync(UserProduct newUserProduct)
        {
            if (!IsAuthenticated) return null;

            var json = JsonSerializer.Serialize(newUserProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("products/user", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.PostAsync("products/user", content);
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

            return await response.Content.ReadFromJsonAsync<UserProduct>();
        }

        public async Task<BaseProduct> AddProductAsync<BaseProduct>(BaseProduct newBaseProduct)
        {
            var json = JsonSerializer.Serialize(newBaseProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("products/base", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.PostAsync("products/base", content);
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

            return await response.Content.ReadFromJsonAsync<BaseProduct>();
        }

        public async Task DeleteUserProductAsync(int id)
        {
            if (!IsAuthenticated) return;

            var response = await _httpClient.DeleteAsync($"products/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                    response = await _httpClient.DeleteAsync($"products/{id}");
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
        #endregion
    }
}

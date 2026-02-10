using DietHelper.Common.Models.Products;
using DietHelper.Data;
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
        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            if (!IsAuthenticated) LoadSavedSession();

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
            if (!IsAuthenticated) LoadSavedSession();

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
            if (!IsAuthenticated) LoadSavedSession();

            Debug.WriteLine($"[GetUserProductAsync] IsAuthenticated: {IsAuthenticated}");
            Debug.WriteLine($"[GetUserProductAsync] Token exists: {!string.IsNullOrEmpty(CurrentSessionData.AccessToken)}");
            Debug.WriteLine($"[GetUserProductAsync] Token: {CurrentSessionData.AccessToken}");

            var response = await _httpClient.GetAsync($"products/{userProductId}");

            Debug.WriteLine($"[GetUserProductAsync] Response Status: {response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Debug.WriteLine($"[GetUserProductAsync] First request 401, refreshing token...");

                if (await RefreshTokensAsync())
                {
                    Debug.WriteLine($"[GetUserProductAsync] Token refreshed, retrying...");

                    response = await _httpClient.GetAsync($"products/{userProductId}");
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Debug.WriteLine($"[GetUserProductAsync] Second request also 401!");
                        return null;
                    }
                }
            }
            else
            {
                Debug.WriteLine($"[GetUserProductAsync] Refresh failed");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[GetUserProductAsync] Final status: {response.StatusCode}");
                return null;
            }

            var userProduct = await response.Content.ReadFromJsonAsync<UserProduct>();

            return userProduct;
        }

        public async Task<UserProduct> AddUserProductAsync(UserProduct newUserProduct)
        {
            if (!IsAuthenticated) LoadSavedSession();

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
            if (!IsAuthenticated)
            {
                LoadSavedSession();
                if (!IsAuthenticated)
                    throw new UnauthorizedAccessException("User not authenticated");
            }

            var json = JsonSerializer.Serialize(newBaseProduct, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("products/base", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Server error response: {errorContent}");
                Debug.WriteLine($"Status: {response.StatusCode}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("Token expired or invalid. Attempting refresh...");
                    if (await RefreshTokensAsync())
                    {
                        if (!string.IsNullOrEmpty(CurrentSessionData.AccessToken))
                            _httpClient.DefaultRequestHeaders.Authorization =
                                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentSessionData.AccessToken);
                        response = await _httpClient.PostAsync("products/base", content);
                    }
                    else throw new UnauthorizedAccessException("Authentication failed");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Server returned {response.StatusCode}: {errorContent}",
                        null, response.StatusCode);
                }
            }

            return await response.Content.ReadFromJsonAsync<BaseProduct>();
        }

        public async Task DeleteUserProductAsync(int id)
        {
            if (!IsAuthenticated) LoadSavedSession();

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
    }
}

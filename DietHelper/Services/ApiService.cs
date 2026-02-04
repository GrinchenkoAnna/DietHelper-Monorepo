using CommunityToolkit.Mvvm.Messaging;
using DietHelper.Common.DTO;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public class SessionData
    {
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private SessionData CurrentSessionData { get; set; } = new SessionData()
        {
            Token = null,
            UserId = -1,
            UserName = null
        };

        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentSessionData.Token);

        public event Action? AuthStateChanged;

        public ApiService()
        {
            Debug.WriteLine($"[ApiService] Создан новый экземпляр. HashCode: {GetHashCode()}");

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5119/api/")
            };

            LoadSavedSession();
        }

        #region Authorization
        private void SetToken(SessionData sessionData)
        {
            CurrentSessionData = sessionData;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentSessionData.Token);

            SaveSession();

            AuthStateChanged?.Invoke();
        }

        private void SaveSession()
        {
            if (string.IsNullOrEmpty(CurrentSessionData.Token))
            {
                Debug.WriteLine($"[ApiService]: token is null");
                return;
            }

            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var folderPath = Path.Combine(appDataPath, "DietHelper");
                Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, "session.json");
                var json = JsonSerializer.Serialize(CurrentSessionData);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
            }
        }

        private void LoadSavedSession()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var filePath = Path.Combine(appDataPath, "DietHelper", "session.json");

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var sessionData = JsonSerializer.Deserialize<SessionData>(json);
                    if (sessionData is not null && !string.IsNullOrEmpty(sessionData.Token))
                        SetToken(sessionData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
            }
        }

        private void EndSession()
        {
            CurrentSessionData.Token = null;
            CurrentSessionData.UserId = -1;
            CurrentSessionData.UserName = null;

            _httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var filePath = Path.Combine(appDataPath, "DietHelper", "session.json");

                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
            }

            AuthStateChanged?.Invoke();
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/register", registerDto);
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                    if (!string.IsNullOrEmpty(authResponse.Token))
                    {
                        SetToken(new SessionData
                        {
                            Token = authResponse.Token,
                            UserId = authResponse.UserId,
                            UserName = authResponse.UserName
                        });

                        return authResponse;
                    }
                }

                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return new AuthResponseDto()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", loginDto);
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (!string.IsNullOrEmpty(authResponse.Token))
                {
                    SetToken(new SessionData
                    {
                        Token = authResponse.Token,
                        UserId = authResponse.UserId,
                        UserName = authResponse.UserName
                    });

                    return authResponse;
                }

                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return new AuthResponseDto()
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public void Logout()
        {
            EndSession();

            Debug.WriteLine("--->" + CurrentSessionData.Token);
        }
        #endregion

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
            return new User()
            {
                Id = CurrentSessionData.UserId
            };
        }

        #region Products
        public async Task<List<UserProduct>?> GetUserProductsAsync()
        {
            if (!IsAuthenticated) return null;

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
            if (!IsAuthenticated) return null;

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

        public async Task<UserProduct?> GetUserProductAsync(int userProductId)
        {
            if (!IsAuthenticated) return null;

            var response = await _httpClient.GetAsync($"products/{userProductId}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var userProduct = await response.Content.ReadFromJsonAsync<UserProduct>();

            return userProduct;
        }

        public async Task<UserProduct?> AddUserProductAsync(UserProduct newUserProduct)
        {
            if (!IsAuthenticated) return null;

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
            if (!IsAuthenticated) return;

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
        public async Task<List<UserDish>?> GetUserDishesAsync()
        {
            if (!IsAuthenticated) return null;

            var response = await _httpClient.GetAsync($"dishes");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var UserDishes = await response.Content.ReadFromJsonAsync<List<UserDish>>();

            return UserDishes;
        }

        public async Task<UserDish?> GetUserDishAsync(int id)
        {
            if (!IsAuthenticated) return null;

            var response = await _httpClient.GetAsync($"dishes/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var UserDish = await response.Content.ReadFromJsonAsync<UserDish>();

            return UserDish;
        }

        public async Task<UserDish?> AddUserDishAsync(UserDish newUserDish)
        {
            if (!IsAuthenticated) return null;

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
            if (!IsAuthenticated) return;

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
            if (!IsAuthenticated) return null;

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
            if (!IsAuthenticated) return false;

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

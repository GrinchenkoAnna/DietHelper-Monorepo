using DietHelper.Common.DTO;
using DietHelper.Common.Models;
using DietHelper.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        private readonly HttpClient _httpClient;
        private SessionData CurrentSessionData { get; set; } = new SessionData()
        {
            AccessToken = null,
            RefreshToken = null,
            UserId = -1,
            UserName = null
        };

        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentSessionData.AccessToken);

        public event Action? AuthStateChanged;

        public ApiService(HttpClient httpClient)
        {
            Debug.WriteLine($"[ApiService] Создан новый экземпляр. HashCode: {GetHashCode()}");

            _httpClient = httpClient;

            LoadSavedSession();
        }

        private void SetTokens(SessionData sessionData)
        {
            CurrentSessionData = sessionData;

            Debug.WriteLine($"[SetTokens] New Access Token: {!string.IsNullOrEmpty(CurrentSessionData.AccessToken)}");
            Debug.WriteLine($"[SetTokens] Header before: {_httpClient.DefaultRequestHeaders.Authorization}");

            if (!string.IsNullOrEmpty(CurrentSessionData.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentSessionData.AccessToken);
                Debug.WriteLine($"[SetTokens] Header after: {_httpClient.DefaultRequestHeaders.Authorization}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                Debug.WriteLine($"[SetTokens] Header cleared");
            }

            SaveSession();

            AuthStateChanged?.Invoke();
        }

        private void SaveSession()
        {
            if (string.IsNullOrEmpty(CurrentSessionData.AccessToken))
            {
                Debug.WriteLine($"[ApiService]: token is null, session not saved");
                return;
            }

            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var folderPath = Path.Combine(appDataPath, "DietHelper");
                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, "session.dat");

                var json = JsonSerializer.Serialize(CurrentSessionData);
                var planeBytes = Encoding.UTF8.GetBytes(json);
                var encryptedBytes = ProtectedData.Protect(planeBytes, null, DataProtectionScope.CurrentUser);

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
                var filePath = Path.Combine(appDataPath, "DietHelper", "session.dat");

                if (!File.Exists(filePath)) return;

                var encryptedBytes = File.ReadAllBytes(filePath);
                var plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(plainBytes);

                var sessionData = JsonSerializer.Deserialize<SessionData>(json);
                if (sessionData is not null &&
                    !string.IsNullOrEmpty(sessionData.AccessToken) &&
                    !string.IsNullOrEmpty(sessionData.RefreshToken))
                    SetTokens(sessionData);

            }
            catch (CryptographicException)
            {
                Debug.WriteLine($"[ApiService]: Failed to decrypt session file");
                LoadLegacySession();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
            }
        }

        private void LoadLegacySession()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var filePath = Path.Combine(appDataPath, "DietHelper", "session.json");

                if (!File.Exists(filePath)) return;

                var json = File.ReadAllText(filePath);
                var sessionData = JsonSerializer.Deserialize<SessionData>(json);
                if (sessionData is not null &&
                    !string.IsNullOrEmpty(sessionData.AccessToken) &&
                    !string.IsNullOrEmpty(sessionData.RefreshToken))
                {
                    SetTokens(sessionData); //-> SaveSession с зашифрованной версией, старый файл не нужен
                    File.Delete(filePath);
                }
                    
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
            }
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/register", registerDto);
                if (response.IsSuccessStatusCode)
                {
                    var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                    if (authResponse is not null && authResponse.IsSuccess &&
                        !string.IsNullOrEmpty(authResponse.AccessToken) &&
                        !string.IsNullOrEmpty(authResponse.RefreshToken))
                    {
                        SetTokens(new SessionData
                        {
                            AccessToken = authResponse.AccessToken,
                            RefreshToken = authResponse.RefreshToken,
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
                if (authResponse is not null && authResponse.IsSuccess &&
                    !string.IsNullOrEmpty(authResponse.AccessToken) &&
                    !string.IsNullOrEmpty(authResponse.RefreshToken))
                {
                    SetTokens(new SessionData
                    {
                        AccessToken = authResponse.AccessToken,
                        RefreshToken = authResponse.RefreshToken,
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

        private async Task<bool> RefreshTokensAsync()
        {
            if (string.IsNullOrEmpty(CurrentSessionData.RefreshToken))
            {
                Debug.WriteLine("[RefreshTokensAsync] No refresh token");
                return false;
            }

            var response = await _httpClient.PostAsJsonAsync("auth/refresh", new { RefreshToken = CurrentSessionData.RefreshToken });

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("[RefreshTokensAsync] Refresh successful");

                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (authResponse is not null)
                {
                    SetTokens(new SessionData
                    {
                        AccessToken = authResponse.AccessToken,
                        RefreshToken = authResponse.RefreshToken,
                        UserId = authResponse.UserId,
                        UserName = authResponse.UserName
                    });

                    return true;
                }
            }

            Debug.WriteLine($"[RefreshTokensAsync] Refresh failed: {response.StatusCode}");
            EndSession();
            // что-то еще сделать?
            return false;
        }

        // удаление токенов на клиенте
        private void EndSession()
        {
            CurrentSessionData.AccessToken = null;
            CurrentSessionData.RefreshToken = null;
            CurrentSessionData.UserId = -1;
            CurrentSessionData.UserName = null;

            _httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var filePath = Path.Combine(appDataPath, "DietHelper", "session.dat");
                if (File.Exists(filePath)) File.Delete(filePath);

                filePath = Path.Combine(appDataPath, "DietHelper", "session.json");
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
            }

            AuthStateChanged?.Invoke();
        }

        public async Task Logout()
        {
            var refreshTokenToRevoke = CurrentSessionData.RefreshToken;

            EndSession();

            // отзыв токенов на сервере
            if (!string.IsNullOrEmpty(refreshTokenToRevoke))
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("auth/logout", new { RefreshToken = refreshTokenToRevoke });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ApiService]: {ex.Message}");
                }
            }
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
            return new User()
            {
                Id = CurrentSessionData.UserId
            };
        }
    }
}

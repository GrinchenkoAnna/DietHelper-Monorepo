using System;
using System.Text.Json.Serialization;

namespace DietHelper.Common.DTO
{
    public class AuthResponseDto
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }
        
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("accessTokenExpiry")]
        public DateTime AccessTokenExpiry { get; set; }

        [JsonPropertyName("refreshTokenExpiry")]
        public DateTime RefreshTokenExpiry { get; set; }
    }
}

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

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("tokenExpiry")]
        public DateTime TokenExpiry { get; set; }
    }
}

namespace DietHelper.Server.DTO.Auth
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
}

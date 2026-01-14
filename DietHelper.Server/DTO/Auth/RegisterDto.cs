using System.ComponentModel.DataAnnotations;

namespace DietHelper.Server.DTO.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Пароль должен быть от 6 до 50 символов")]
        public string Password { get; set; } = string.Empty;
    }
}

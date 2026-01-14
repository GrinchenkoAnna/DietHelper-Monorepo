using System.ComponentModel.DataAnnotations;

namespace DietHelper.Server.DTO.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Логин обязателен")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }
}

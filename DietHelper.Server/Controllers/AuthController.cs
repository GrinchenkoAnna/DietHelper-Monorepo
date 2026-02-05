using DietHelper.Common.DTO;
using DietHelper.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var exitingUser = await _userManager.FindByNameAsync(registerDto.UserName);
                if (exitingUser is not null)
                    return BadRequest(new AuthResponseDto()
                    {
                        Message = "Пользователь с таким логином уже существует",
                        IsSuccess = false
                    });

                var newUser = new User { UserName = registerDto.UserName };
                var result = await _userManager.CreateAsync(newUser, registerDto.Password); //Identity хеширует пароль

                if (!result.Succeeded)
                    return BadRequest(new AuthResponseDto()
                    {
                        Message = string.Join(", ", result.Errors.Select(err => err.Description)),
                        IsSuccess = false
                    });

                var token = GenerateJwtToken(newUser);

                return Ok(new AuthResponseDto()
                {
                    IsSuccess = true,
                    Message = "Регистрация прошла успешно",
                    Token = token,
                    UserId = newUser.Id,
                    UserName = newUser.UserName,
                    TokenExpiry = DateTime.UtcNow.AddHours(2)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[AuthController]: {ex.Message}");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName!)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var exitingUser = await _userManager.FindByNameAsync(loginDto.UserName);
                if (exitingUser is null)
                    return Unauthorized(new AuthResponseDto()
                    {
                        Message = "Неверный логин или пароль",
                        IsSuccess = false
                    });

                var isPasswordCorrect = await _userManager.CheckPasswordAsync(exitingUser, loginDto.Password);
                if (!isPasswordCorrect)
                    return Unauthorized(new AuthResponseDto()
                    {
                        Message = "Неверный логин или пароль",
                        IsSuccess = false
                    });

                var token = GenerateJwtToken(exitingUser);

                return Ok(new AuthResponseDto()
                {
                    IsSuccess = true,
                    Message = "Регистрация прошла успешно",
                    Token = token,
                    UserId = exitingUser.Id,
                    UserName = exitingUser.UserName,
                    TokenExpiry = DateTime.UtcNow.AddHours(2)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[AuthController]: {ex.Message}");
            }
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            return Ok(new { message = "Вы успешно вышли из DietHelper" });
        }
    }
}

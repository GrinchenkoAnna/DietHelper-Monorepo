using DietHelper.Common.DTO;
using DietHelper.Common.Models;
using DietHelper.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<User> userManager, IConfiguration configuration, ITokenService tokenService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto request)
        {
            try
            {
                var exitingUser = await _userManager.FindByNameAsync(request.UserName);
                if (exitingUser is not null)
                    return BadRequest(new AuthResponseDto()
                    {
                        Message = "Пользователь с таким логином уже существует",
                        IsSuccess = false
                    });

                var newUser = new User { UserName = request.UserName };
                var result = await _userManager.CreateAsync(newUser, request.Password); //Identity хеширует пароль

                if (!result.Succeeded)
                    return BadRequest(new AuthResponseDto()
                    {
                        Message = string.Join(", ", result.Errors.Select(err => err.Description)),
                        IsSuccess = false
                    });

                var accessToken = await _tokenService.GenerateAccessTokenAsync(exitingUser);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(exitingUser);

                return Ok(new AuthResponseDto()
                {
                    IsSuccess = true,
                    Message = "Регистрация прошла успешно",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = exitingUser.Id,
                    UserName = exitingUser.UserName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[AuthController]: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto request)
        {
            try
            {
                var exitingUser = await _userManager.FindByNameAsync(request.UserName);
                if (exitingUser is null)
                    return Unauthorized(new AuthResponseDto()
                    {
                        Message = "Неверный логин или пароль",
                        IsSuccess = false
                    });

                var isPasswordCorrect = await _userManager.CheckPasswordAsync(exitingUser, request.Password);
                if (!isPasswordCorrect)
                    return Unauthorized(new AuthResponseDto()
                    {
                        Message = "Неверный логин или пароль",
                        IsSuccess = false
                    });

                var accessToken = await _tokenService.GenerateAccessTokenAsync(exitingUser);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(exitingUser);

                return Ok(new AuthResponseDto()
                {
                    IsSuccess = true,
                    Message = "Регистрация прошла успешно",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = exitingUser.Id,
                    UserName = exitingUser.UserName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[AuthController]: {ex.Message}");
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto request)
        {
            if (!await _tokenService.IsRefreshTokenValidAsync(request.RefreshToken))
                return Unauthorized(new AuthResponseDto()
                {
                    IsSuccess = false,
                    Message = "Недействительный или отозванный токен"
                });

            var refreshToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken is null)
                return Unauthorized(new AuthResponseDto()
                {
                    IsSuccess = false,
                    Message = "Токен не найден"
                });

            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(refreshToken.User);
            await _tokenService.RevokeRefreshTokenAsync(refreshToken.Token);

            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(refreshToken.User);

            return Ok(new AuthResponseDto()
            {
                IsSuccess = true,
                RefreshToken = newRefreshToken,
                AccessToken = newAccessToken,
                UserId = refreshToken.UserId,
                UserName = refreshToken.User.UserName
            });
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutDto request)
        {
            if (!string.IsNullOrEmpty(request.RefreshToken))
                await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            return Ok("Вы успешно вышли из DietHelper");
        }
    }
}

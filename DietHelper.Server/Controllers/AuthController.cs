using DietHelper.Common.Models;
using DietHelper.Server.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
                //!
                return Ok(new AuthResponseDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[AuthController]: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                //!
                return Ok(new AuthResponseDto());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[AuthController]: {ex.Message}");
            }
        }

    }
}

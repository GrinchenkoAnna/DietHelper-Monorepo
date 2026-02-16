using DietHelper.Common.Data;
using DietHelper.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DietHelper.Server.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User user);
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenString);
        Task RevokeRefreshTokenAsync(string refreshTokenString);
        Task<bool> IsRefreshTokenValidAsync(string refreshTokenString);
        Task CleanExpiredRefreshTokensAsync();
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly DietHelperDbContext _dbContext;

        public TokenService(IConfiguration configuration, DietHelperDbContext dietHelperDbContext)
        {
            _configuration = configuration;
            _dbContext = dietHelperDbContext;
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var randomNumber = new byte[32];
            var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            string refreshTokenString = Convert.ToBase64String(randomNumber);

            var refreshToken = new RefreshToken()
            {
                Token = refreshTokenString,
                User = user,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshTokenString;
        }
        public Task<RefreshToken?> GetRefreshTokenAsync(string refreshTokenString)
        {
            return _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenString);
        }

        public async Task<bool> IsRefreshTokenValidAsync(string refreshTokenString)
        {
            var refreshToken = await GetRefreshTokenAsync(refreshTokenString);
            return refreshToken != null && refreshToken.IsActive;
        }

        public async Task RevokeRefreshTokenAsync(string refreshTokenString)
        {
            var refreshToken = await GetRefreshTokenAsync(refreshTokenString);
            if (refreshToken is null) return;

            refreshToken.RevokedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
        public async Task CleanExpiredRefreshTokensAsync()
        {
            var expirationDate = DateTime.UtcNow.AddDays(-7);

            var expiredRefreshTokens = await _dbContext.RefreshTokens
                .Where(rt => rt.ExpiresAt < expirationDate)
                .ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(expiredRefreshTokens);

            await _dbContext.SaveChangesAsync();
        }
    }
}

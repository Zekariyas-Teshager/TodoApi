using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly TodoContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            TodoContext context
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Registration failed: {errors}");
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "User");

            // Generate tokens for automatic login after registration
            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Refresh token is required");
            }

            // Find the refresh token in database
            var refreshTokenEntity = await _context
                .RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (refreshTokenEntity == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            if (refreshTokenEntity.IsRevoked)
            {
                // If token is revoked, revoke its descendants (security measure)
                await RevokeDescendantTokensAsync(refreshTokenEntity);
                throw new UnauthorizedAccessException("Refresh token has been revoked");
            }

            if (refreshTokenEntity.ExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token has expired");
            }

            // Generate new tokens (token rotation)
            var user = refreshTokenEntity.User;
            var newRefreshToken = await GenerateRefreshTokenAsync(
                user,
                refreshTokenEntity.CreatedByIp?.ToString() ?? "unknown"
            );

            // Revoke current token and mark it as replaced
            refreshTokenEntity.IsRevoked = true;
            refreshTokenEntity.ReplacedByToken = newRefreshToken.Token;

            _context.RefreshTokens.Update(refreshTokenEntity);
            await _context.RefreshTokens.AddAsync(newRefreshToken);
            await _context.SaveChangesAsync();

            // Generate new access token
            var accessToken = await GenerateAccessTokenAsync(user);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = newRefreshToken.Token,
            };
        }

        public async Task<bool> RevokeTokenAsync(string token, string userId, bool isAdmin)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Refresh token is required");
            }

            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt =>
                rt.Token == token
            );

            if (refreshToken == null)
            {
                return false;
            }

            // Check if the user owns this token or is admin
            if (!isAdmin && refreshToken.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You don't have permission to revoke this token"
                );
            }

            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            // Revoke all active refresh tokens for this user
            var activeTokens = await _context
                .RefreshTokens.Where(rt =>
                    rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow
                )
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public string GetClientIpAddress(HttpContext httpContext)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        #region Private Helper Methods

        private async Task<AuthResponseDto> GenerateAuthResponseAsync(ApplicationUser user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user, "unknown"); // IP will be set by controller

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDto { Token = accessToken, RefreshToken = refreshToken.Token };
        }

        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new("userId", user.Id),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["DurationInMinutes"])
                ),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(
            ApplicationUser user,
            string ipAddress
        )
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
            };
        }

        private async Task RevokeDescendantTokensAsync(RefreshToken token)
        {
            var currentToken = token;
            while (!string.IsNullOrEmpty(currentToken.ReplacedByToken))
            {
                var nextToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt =>
                    rt.Token == currentToken.ReplacedByToken
                );

                if (nextToken != null && !nextToken.IsRevoked)
                {
                    nextToken.IsRevoked = true;
                    _context.RefreshTokens.Update(nextToken);
                }

                currentToken = nextToken;
                if (currentToken == null)
                    break;
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}

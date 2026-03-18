using TodoApi.DTOs;

namespace TodoApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string token, string userId, bool isAdmin);
        Task<bool> LogoutAsync(string userId);
        string GetClientIpAddress(HttpContext httpContext);
    }
}
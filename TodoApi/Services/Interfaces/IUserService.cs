using TodoApi.DTOs;
using Microsoft.AspNetCore.Identity;

namespace TodoApi.Services.Interfaces
{
    public interface IUserService
    {
        // Admin methods
        Task<IEnumerable<UserProfileDto>> GetAllUsersAsync();

        // User profile methods
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UpdateProfileDto updateDto, bool isAdmin);

        // Token methods
        Task<IEnumerable<TokenInfoDto>> GetUserTokensAsync(string userId);
        Task<IEnumerable<AdminUserTokenInfoDto>> GetUserTokensByAdminAsync(string userId);
        Task<bool> UserExistsAsync(string userId);

        // Password management
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto, bool isAdmin);
    }
}
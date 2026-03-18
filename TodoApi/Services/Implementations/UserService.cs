using Microsoft.EntityFrameworkCore;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly TodoContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(TodoContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Admin Methods

        public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved all {Count} users (Admin access)", users.Count);
            return users;
        }

        public async Task<IEnumerable<AdminUserTokenInfoDto>> GetUserTokensByAdminAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Include(rt => rt.User)
                .Where(rt => rt.UserId == userId)
                .Select(rt => new AdminUserTokenInfoDto
                {
                    Id = rt.Id,
                    Token = rt.Token,
                    ExpiryDate = rt.ExpiryDate,
                    IsRevoked = rt.IsRevoked,
                    ReplacedByToken = rt.ReplacedByToken,
                    CreatedAt = rt.CreatedAt,
                    CreatedByIp = rt.CreatedByIp,
                    User = rt.User != null ? new UserProfileDto
                    {
                        Id = rt.User.Id,
                        Email = rt.User.Email ?? string.Empty,
                        FullName = rt.User.FullName
                    } : null
                })
                .ToListAsync();

            _logger.LogInformation("Admin retrieved {Count} tokens for user {UserId}", tokens.Count, userId);
            return tokens;
        }

        #endregion

        #region User Profile Methods

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning("User profile not found for ID {UserId}", userId);
                return null;
            }

            _logger.LogInformation("Retrieved profile for user {UserId}", userId);
            return user;
        }

        public async Task<UserProfileDto?> UpdateUserProfileAsync(string userId, UpdateProfileDto updateDto, bool isAdmin)
        {
            var existingUser = await _context.Users.FindAsync(userId);
            
            if (existingUser == null)
            {
                _logger.LogWarning("User not found for update: {UserId}", userId);
                return null;
            }

            // Check permission (already checked in controller, but double-check)
            if (!isAdmin && existingUser.Id != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update another user's profile", userId);
                throw new UnauthorizedAccessException("You don't have permission to update this profile");
            }

            // Update properties if provided
            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(updateDto.FullName) && updateDto.FullName != existingUser.FullName)
            {
                existingUser.FullName = updateDto.FullName;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != existingUser.Email)
            {
                // Check if email is already taken
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == updateDto.Email && u.Id != userId);
                
                if (emailExists)
                {
                    throw new InvalidOperationException($"Email {updateDto.Email} is already in use");
                }

                existingUser.Email = updateDto.Email;
                existingUser.UserName = updateDto.Email; // Keep UserName in sync with Email
                hasChanges = true;
            }

            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for user {UserId}", userId);
                return await GetUserProfileAsync(userId);
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Profile updated for user {UserId}", userId);

            return await GetUserProfileAsync(userId);
        }

        #endregion

        #region Token Methods

        public async Task<IEnumerable<TokenInfoDto>> GetUserTokensAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .Select(rt => new TokenInfoDto
                {
                    Token = rt.Token,
                    ExpiryDate = rt.ExpiryDate,
                    IsRevoked = rt.IsRevoked,
                    ReplacedByToken = rt.ReplacedByToken,
                    CreatedAt = rt.CreatedAt,
                    CreatedByIp = rt.CreatedByIp
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} tokens for user {UserId}", tokens.Count, userId);
            return tokens;
        }

        #endregion

        #region Helper Methods

        public async Task<bool> UserExistsAsync(string userId)
        {
            return await _context.Users.AnyAsync(u => u.Id == userId);
        }

        #endregion
    }
}
// Controllers/UserController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoApi.DTOs;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [Authorize(Roles = "User")]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            
            var user = await _userService.GetUserProfileAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { message = "User profile not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [Authorize(Roles = "User")]
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            try
            {
                var updatedUser = await _userService.UpdateUserProfileAsync(userId, model, isAdmin);
                
                if (updatedUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(updatedUser);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's refresh tokens
        /// </summary>
        [Authorize]
        [HttpGet("tokens")]
        [ProducesResponseType(typeof(IEnumerable<TokenInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyTokens()
        {
            var userId = GetCurrentUserId();
            
            var tokens = await _userService.GetUserTokensAsync(userId);
            
            return Ok(tokens);
        }

        /// <summary>
        /// Get a specific user's tokens (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/tokens/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<AdminUserTokenInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserTokens(string userId)
        {
            // Check if user exists
            var userExists = await _userService.UserExistsAsync(userId);
            if (!userExists)
            {
                return NotFound(new { message = $"User with ID {userId} not found" });
            }

            var tokens = await _userService.GetUserTokensByAdminAsync(userId);
            
            return Ok(tokens);
        }


        /// <summary>
        /// Change current user's password
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid input or password change failed</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">User not found</response>
        /// <remarks>
        /// This endpoint allows users to change their password. Regular users can only change their own password, while admins can change any user's password. The request body should contain the current password and the new password. The new password must meet the application's password policy requirements.
        /// </remarks>
        /// <example>
        /// POST /api/user/change-password
        /// {
        ///   "currentPassword": "P@ssw0rd!",
        ///  "newPassword": "N3wP@ssw0rd!"
        /// }
        /// </example>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            try
            {
                var result = await _userService.ChangePasswordAsync(userId, changePasswordDto, isAdmin);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Password change failed", errors = result.Errors.Select(e => e.Description) });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #region Private Helper Methods

        private string GetCurrentUserId()
        {
            return User.FindFirstValue("userId") ?? 
                   User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                   throw new UnauthorizedAccessException("User ID not found in token");
        }

        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        #endregion
    }
}
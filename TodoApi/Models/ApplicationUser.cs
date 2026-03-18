// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace TodoApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        // You can add custom properties here if needed
        public string? FullName { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
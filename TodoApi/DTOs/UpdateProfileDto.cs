namespace TodoApi.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

    public class TokenInfoDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public string? ReplacedByToken { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByIp { get; set; }
    }

    public class AdminUserTokenInfoDto : TokenInfoDto
    {
        public int Id { get; set; }
        public UserProfileDto? User { get; set; }
    }
}
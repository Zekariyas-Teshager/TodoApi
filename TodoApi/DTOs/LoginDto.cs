namespace TodoApi.DTOs
{
    public class LoginDto
    {
        /// <summary>
        /// The email address of the user trying to log in. This should be a valid email format and is required for authentication.
        /// </summary>
        /// Example: "user@example.com"
        public string Email { get; set; } = string.Empty;

        /// <summary> The password of the user trying to log in. This is a required field and should be kept secure. It is used in conjunction with the email to authenticate the user and grant access to protected resources. </summary>
        /// Example: "P@ssw0rd!"
        public string Password { get; set; } = string.Empty;
    }
}
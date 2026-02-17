namespace BasarMapApp.Api.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Error message for failed login (e.g. lockout, wrong password with remaining attempts)
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Auth
{
    public class UserLoginDto
    {
        [Required]
        public string LoginIdentifier { get; set; } = string.Empty; // Username or Email
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

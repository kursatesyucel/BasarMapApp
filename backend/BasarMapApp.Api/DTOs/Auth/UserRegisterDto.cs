using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Auth
{
    public class UserRegisterDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}

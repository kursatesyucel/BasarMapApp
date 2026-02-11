using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Auth
{
    public class UpdateUserRoleDto
    {
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;
    }
}

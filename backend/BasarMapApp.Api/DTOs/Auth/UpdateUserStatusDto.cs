using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Auth
{
    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}

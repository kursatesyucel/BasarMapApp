using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Camera
{
    public class CreateCameraDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }
        
        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string VideoFileName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
} 
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Models
{
    public class Camera
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public Point Geometry { get; set; } = null!;
        
        [Required]
        [MaxLength(255)]
        public string VideoFileName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
} 
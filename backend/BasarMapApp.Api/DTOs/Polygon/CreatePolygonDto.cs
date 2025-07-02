using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Polygon
{
    public class CreatePolygonDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Polygon must have at least one ring")]
        public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
    }
} 
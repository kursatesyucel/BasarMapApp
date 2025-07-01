using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.DTOs.Line
{
    public class CreateLineDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(2, ErrorMessage = "Line must have at least 2 points")]
        public List<List<double>> Coordinates { get; set; } = new List<List<double>>();
    }
} 
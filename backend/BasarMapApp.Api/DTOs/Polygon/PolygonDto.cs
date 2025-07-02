namespace BasarMapApp.Api.DTOs.Polygon
{
    public class PolygonDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<List<List<double>>> Coordinates { get; set; } = new List<List<List<double>>>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 
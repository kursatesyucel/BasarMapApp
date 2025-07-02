using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Models
{
    public class MapLine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public LineString Geometry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 
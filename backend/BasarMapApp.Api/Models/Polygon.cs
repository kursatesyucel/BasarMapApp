using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Models
{
    public class MapPolygon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Polygon Geometry { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 
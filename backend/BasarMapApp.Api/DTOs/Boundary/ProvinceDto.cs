using System.Text.Json.Serialization;

namespace BasarMapApp.Api.DTOs.Boundary
{
    /// <summary>
    /// İl Sınırları DTO - GeoJSON formatında geometry ile
    /// Sadece görsel sınır katmanı
    /// </summary>
    public class ProvinceDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        /// <summary>
        /// GeoJSON formatında geometri
        /// ST_SimplifyPreserveTopology ile optimize edilmiş
        /// </summary>
        [JsonPropertyName("geometry")]
        public object Geometry { get; set; } = null!;
    }
}

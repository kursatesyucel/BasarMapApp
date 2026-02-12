using System.Text.Json.Serialization;

namespace BasarMapApp.Api.DTOs.Boundary
{
    /// <summary>
    /// İlçe Sınırları DTO - GeoJSON formatında geometry ile
    /// </summary>
    public class DistrictDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// GeoJSON formatında geometri
        /// ST_SimplifyPreserveTopology ile optimize edilmiş
        /// </summary>
        [JsonPropertyName("geometry")]
        public object Geometry { get; set; } = null!;
    }
}

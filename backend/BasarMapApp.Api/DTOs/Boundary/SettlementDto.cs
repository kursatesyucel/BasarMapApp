using System.Text.Json.Serialization;

namespace BasarMapApp.Api.DTOs.Boundary
{
    /// <summary>
    /// Yerleşim Merkezi DTO - GeoJSON formatında geometry ile
    /// </summary>
    public class SettlementDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// GeoJSON formatında Point geometri
        /// </summary>
        [JsonPropertyName("geometry")]
        public object Geometry { get; set; } = null!;
        
        /// <summary>
        /// Koordinat bilgileri (hızlı erişim için)
        /// </summary>
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
    }
}

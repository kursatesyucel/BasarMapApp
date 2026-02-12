using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Models
{
    /// <summary>
    /// İl Sınırları (Province Boundaries) referans verisi modeli
    /// Read-only GIS referans tablosu: ref_provinces
    /// Sadece görsel sınır katmanı - detay_adı her zaman "İL_SINIRI"
    /// </summary>
    [Table("ref_provinces")]
    public class Province
    {
        /// <summary>
        /// Primary Key (OGC Feature ID)
        /// </summary>
        [Key]
        [Column("ogc_fid")]
        public int Id { get; set; }
        
        /// <summary>
        /// Detay adı (veritabanında her zaman "İL_SINIRI")
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("detay_adı")]
        public string DetailName { get; set; } = string.Empty;
        
        /// <summary>
        /// Geometri (MultiPolygon - SRID 4326)
        /// İl sınır poligonları
        /// </summary>
        [Required]
        [Column("geom", TypeName = "geometry")]
        public Geometry Geom { get; set; } = null!;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Models
{
    /// <summary>
    /// İlçe Sınırları (District Boundaries) referans verisi modeli
    /// Read-only GIS referans tablosu: ref_districts
    /// Bağımsız tablo - Navigation Property YOK
    /// </summary>
    [Table("ref_districts")]
    public class District
    {
        /// <summary>
        /// Primary Key (OGC Feature ID)
        /// </summary>
        [Key]
        [Column("ogc_fid")]
        public int Id { get; set; }
        
        /// <summary>
        /// İlçe adı (veritabanında detay_adı sütunu - Türkçe karakter)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("detay_adı")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Geometri (MultiPolygon - SRID 4326)
        /// İlçe sınır poligonları
        /// </summary>
        [Required]
        [Column("geom", TypeName = "geometry")]
        public Geometry Geom { get; set; } = null!;
    }
}

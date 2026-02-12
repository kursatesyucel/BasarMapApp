using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Models
{
    /// <summary>
    /// Yerleşim Merkezi (Settlement Center) referans verisi modeli
    /// Read-only GIS referans tablosu: ref_settlements
    /// Kategoriler: BAŞKENT, İL, İLÇE
    /// Bağımsız tablo - Navigation Property YOK
    /// </summary>
    [Table("ref_settlements")]
    public class Settlement
    {
        /// <summary>
        /// Primary Key (OGC Feature ID)
        /// </summary>
        [Key]
        [Column("ogc_fid")]
        public int Id { get; set; }
        
        /// <summary>
        /// Yerleşim merkezi adı (veritabanında adı sütunu - Türkçe karakter)
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("adı")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Kategori: BAŞKENT, İL, İLÇE
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("kategori")]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Geometri (Point - SRID 4326)
        /// Merkez noktası koordinatları
        /// </summary>
        [Required]
        [Column("geom", TypeName = "geometry(Point,4326)")]
        public Point Geom { get; set; } = null!;
    }
}

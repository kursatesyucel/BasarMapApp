using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MapPoint> Points { get; set; }
        public DbSet<MapLine> Lines { get; set; }
        public DbSet<MapPolygon> Polygons { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<User> Users { get; set; }
        
        // GIS Referans Verileri (Read-Only)
        public DbSet<Province> Provinces { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Settlement> Settlements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // MapPoint configuration
            builder.Entity<MapPoint>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                
                entity.Property(e => e.Geometry)
                    .IsRequired()
                    .HasColumnType("geometry (point, 4326)");
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.UpdatedAt);
            });

            // MapLine configuration
            builder.Entity<MapLine>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                
                entity.Property(e => e.Geometry)
                    .IsRequired()
                    .HasColumnType("geometry (linestring, 4326)");
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.UpdatedAt);
            });

            // MapPolygon configuration
            builder.Entity<MapPolygon>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                
                entity.Property(e => e.Geometry)
                    .IsRequired()
                    .HasColumnType("geometry (polygon, 4326)");
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.UpdatedAt);
            });

            // Camera configuration
            builder.Entity<Camera>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
                
                entity.Property(e => e.Geometry)
                    .IsRequired()
                    .HasColumnType("geometry (point, 4326)");
                
                entity.Property(e => e.VideoFileName)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                
                entity.Property(e => e.UpdatedAt);
            });

            // User configuration
            builder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.HasIndex(e => e.Username)
                    .IsUnique();
                
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                
                entity.HasIndex(e => e.Email)
                    .IsUnique();
                
                entity.Property(e => e.PasswordHash)
                    .IsRequired();
                
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20);
                
                entity.Property(e => e.VerificationCode)
                    .HasMaxLength(10);
                
                entity.Property(e => e.IsEmailConfirmed)
                    .IsRequired()
                    .HasDefaultValue(false);
                
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });

            // ============= GIS Referans Verileri Konfigürasyonları =============

            // Province (İl Sınırları) configuration
            builder.Entity<Province>(entity =>
            {
                entity.ToTable("ref_provinces");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("ogc_fid");
                
                entity.Property(e => e.DetailName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("detay_adı");  // Türkçe karakter - her zaman "İL_SINIRI"
                
                entity.Property(e => e.Geom)
                    .IsRequired()
                    .HasColumnName("geom")
                    .HasColumnType("geometry");
                
                // Navigation Property YOK - bağımsız tablo
            });

            // District (İlçe Sınırları) configuration
            builder.Entity<District>(entity =>
            {
                entity.ToTable("ref_districts");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("ogc_fid");
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("detay_adı");  // Türkçe karakter
                
                entity.Property(e => e.Geom)
                    .IsRequired()
                    .HasColumnName("geom")
                    .HasColumnType("geometry");
                
                // Navigation Property YOK - bağımsız tablo
            });

            // Settlement (Yerleşim Merkezi) configuration
            builder.Entity<Settlement>(entity =>
            {
                entity.ToTable("ref_settlements");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .HasColumnName("ogc_fid");
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("adı");  // Türkçe karakter
                
                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("kategori");  // BAŞKENT, İL, İLÇE
                
                entity.Property(e => e.Geom)
                    .IsRequired()
                    .HasColumnName("geom")
                    .HasColumnType("geometry(Point,4326)");
                
                // Navigation Property YOK - bağımsız tablo
            });
        }
    }
}

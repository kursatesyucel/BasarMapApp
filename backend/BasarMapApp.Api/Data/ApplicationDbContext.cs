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
                
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });
        }
    }
}

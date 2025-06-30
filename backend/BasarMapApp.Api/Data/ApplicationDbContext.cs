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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Eğer SRID ya da özel konfigürasyon gerekirse buraya ekleyebilirsiniz
        }
    }
}

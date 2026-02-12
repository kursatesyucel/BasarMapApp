using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Implementations
{
    /// <summary>
    /// Yerleşim Merkezi repository implementasyonu
    /// Kategori filtreleme ve Bounding Box desteği
    /// </summary>
    public class SettlementRepository : ISettlementRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public SettlementRepository(ApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<IEnumerable<Settlement>> GetAllSettlementsAsync()
        {
            return await _context.Settlements
                .AsNoTracking()
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetSettlementsByCategoryAsync(string category)
        {
            // Kategori: BAŞKENT, İL, İLÇE
            return await _context.Settlements
                .AsNoTracking()
                .Where(s => s.Category == category)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Settlement>> GetSettlementsInBoundingBoxAsync(
            double minLon, double minLat, double maxLon, double maxLat)
        {
            // Bounding Box oluştur (SRID 4326)
            var bbox = _geometryFactory.CreatePolygon(new[]
            {
                new Coordinate(minLon, minLat),
                new Coordinate(maxLon, minLat),
                new Coordinate(maxLon, maxLat),
                new Coordinate(minLon, maxLat),
                new Coordinate(minLon, minLat) // Kapalı poligon
            });

            // ST_Intersects ile sadece görünür alandaki noktaları getir
            return await _context.Settlements
                .AsNoTracking()
                .Where(s => s.Geom.Intersects(bbox))
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Settlement?> GetSettlementByIdAsync(int id)
        {
            return await _context.Settlements
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}

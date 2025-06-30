using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Implementations
{
    public class PointRepository : IPointRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public PointRepository(ApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<IEnumerable<MapPoint>> GetAllAsync()
        {
            return await _context.Points
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<MapPoint?> GetByIdAsync(int id)
        {
            return await _context.Points
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<MapPoint> CreateAsync(MapPoint point)
        {
            point.CreatedAt = DateTime.UtcNow;
            point.UpdatedAt = null;
            
            _context.Points.Add(point);
            await _context.SaveChangesAsync();
            
            return point;
        }

        public async Task<MapPoint?> UpdateAsync(int id, MapPoint point)
        {
            var existingPoint = await GetByIdAsync(id);
            if (existingPoint == null)
                return null;

            existingPoint.Name = point.Name;
            existingPoint.Description = point.Description;
            existingPoint.Geometry = point.Geometry;
            existingPoint.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPoint;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var point = await GetByIdAsync(id);
            if (point == null)
                return false;

            _context.Points.Remove(point);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Points.AnyAsync(p => p.Id == id);
        }
    }
}

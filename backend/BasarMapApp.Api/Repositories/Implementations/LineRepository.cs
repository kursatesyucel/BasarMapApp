using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Implementations
{
    public class LineRepository : ILineRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public LineRepository(ApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<IEnumerable<MapLine>> GetAllAsync()
        {
            return await _context.Lines
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<MapLine?> GetByIdAsync(int id)
        {
            return await _context.Lines
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<MapLine> CreateAsync(MapLine line)
        {
            line.CreatedAt = DateTime.UtcNow;
            line.UpdatedAt = null;
            
            _context.Lines.Add(line);
            await _context.SaveChangesAsync();
            
            return line;
        }

        public async Task<MapLine?> UpdateAsync(int id, MapLine line)
        {
            var existingLine = await GetByIdAsync(id);
            if (existingLine == null)
                return null;

            existingLine.Name = line.Name;
            existingLine.Description = line.Description;
            existingLine.Geometry = line.Geometry;
            existingLine.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingLine;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var line = await GetByIdAsync(id);
            if (line == null)
                return false;

            _context.Lines.Remove(line);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Lines
                .AnyAsync(l => l.Id == id);
        }
    }
} 
using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Implementations
{
    public class PolygonRepository : IPolygonRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public PolygonRepository(ApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<IEnumerable<MapPolygon>> GetAllAsync()
        {
            return await _context.Polygons
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<MapPolygon?> GetByIdAsync(int id)
        {
            return await _context.Polygons
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<MapPolygon> CreateAsync(MapPolygon polygon)
        {
            polygon.CreatedAt = DateTime.UtcNow;
            polygon.UpdatedAt = null;
            
            _context.Polygons.Add(polygon);
            await _context.SaveChangesAsync();
            
            return polygon;
        }

        public async Task<MapPolygon?> UpdateAsync(int id, MapPolygon polygon)
        {
            var existingPolygon = await GetByIdAsync(id);
            if (existingPolygon == null)
                return null;

            existingPolygon.Name = polygon.Name;
            existingPolygon.Description = polygon.Description;
            existingPolygon.Geometry = polygon.Geometry;
            existingPolygon.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPolygon;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var polygon = await GetByIdAsync(id);
            if (polygon == null)
                return false;

            _context.Polygons.Remove(polygon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Polygons
                .AnyAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<MapPolygon>> GetIntersectingPolygonsAsync(Polygon polygon)
        {
            return await _context.Polygons
                .Where(p => p.Geometry.Intersects(polygon))
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Polygon?> GetDifferenceFromExistingPolygonsAsync(Polygon newPolygon)
        {
            var intersectingPolygons = await GetIntersectingPolygonsAsync(newPolygon);
            
            if (!intersectingPolygons.Any())
            {
                // No intersections, return original polygon
                return newPolygon;
            }

            var resultPolygon = newPolygon;
            
            foreach (var existingPolygon in intersectingPolygons)
            {
                try
                {
                    // Calculate difference (remove intersection from new polygon)
                    var difference = resultPolygon.Difference(existingPolygon.Geometry);
                    
                    if (difference.IsEmpty)
                    {
                        // New polygon is completely contained within existing polygon
                        return null;
                    }
                    
                    if (difference is Polygon poly)
                    {
                        resultPolygon = poly;
                    }
                    else if (difference is MultiPolygon multiPoly && multiPoly.NumGeometries > 0)
                    {
                        // Take the largest polygon from the result
                        var largestPolygon = multiPoly.Geometries
                            .Cast<Polygon>()
                            .OrderByDescending(p => p.Area)
                            .FirstOrDefault();
                        
                        if (largestPolygon != null)
                        {
                            resultPolygon = largestPolygon;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // Unexpected geometry type or empty result
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception and continue with next polygon
                    // In a real application, you'd use proper logging
                    Console.WriteLine($"Error calculating difference: {ex.Message}");
                    continue;
                }
            }

            return resultPolygon;
        }
    }
} 
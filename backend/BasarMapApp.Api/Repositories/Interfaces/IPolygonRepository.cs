using BasarMapApp.Api.Models;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    public interface IPolygonRepository
    {
        Task<IEnumerable<MapPolygon>> GetAllAsync();
        Task<MapPolygon?> GetByIdAsync(int id);
        Task<MapPolygon> CreateAsync(MapPolygon polygon);
        Task<MapPolygon?> UpdateAsync(int id, MapPolygon polygon);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<MapPolygon>> GetIntersectingPolygonsAsync(Polygon polygon);
        Task<Polygon?> GetDifferenceFromExistingPolygonsAsync(Polygon newPolygon);
    }
} 
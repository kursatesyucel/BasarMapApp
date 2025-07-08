using BasarMapApp.Api.Models;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    public interface ICameraRepository
    {
        Task<IEnumerable<Camera>> GetAllAsync();
        Task<Camera?> GetByIdAsync(int id);
        Task<Camera> CreateAsync(Camera camera);
        Task<Camera?> UpdateAsync(int id, Camera camera);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Camera>> GetActiveCamerasAsync();
        Task<Camera?> GetByVideoFileNameAsync(string videoFileName);
        Task<IEnumerable<Camera>> GetCamerasWithinPolygonAsync(Polygon polygon);
    }
} 
using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    public interface IPointRepository
    {
        Task<IEnumerable<MapPoint>> GetAllAsync();
        Task<MapPoint?> GetByIdAsync(int id);
        Task<MapPoint> CreateAsync(MapPoint point);
        Task<MapPoint?> UpdateAsync(int id, MapPoint point);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}

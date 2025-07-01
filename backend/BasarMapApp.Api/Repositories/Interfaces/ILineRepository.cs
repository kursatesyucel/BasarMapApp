using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    public interface ILineRepository
    {
        Task<IEnumerable<MapLine>> GetAllAsync();
        Task<MapLine?> GetByIdAsync(int id);
        Task<MapLine> CreateAsync(MapLine line);
        Task<MapLine?> UpdateAsync(int id, MapLine line);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
} 
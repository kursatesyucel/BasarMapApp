using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    /// <summary>
    /// Yerleşim Merkezi verilerine erişim için repository interface
    /// Read-only işlemler için tasarlanmıştır
    /// </summary>
    public interface ISettlementRepository
    {
        /// <summary>
        /// Tüm yerleşim merkezlerini getirir
        /// </summary>
        Task<IEnumerable<Settlement>> GetAllSettlementsAsync();
        
        /// <summary>
        /// Kategoriye göre yerleşim merkezlerini filtreler
        /// Kategoriler: BAŞKENT, İL, İLÇE
        /// </summary>
        Task<IEnumerable<Settlement>> GetSettlementsByCategoryAsync(string category);
        
        /// <summary>
        /// Bounding Box içindeki yerleşim merkezlerini getirir
        /// ST_Intersects kullanarak sadece görünür alandaki noktaları döndürür
        /// </summary>
        Task<IEnumerable<Settlement>> GetSettlementsInBoundingBoxAsync(
            double minLon, double minLat, double maxLon, double maxLat);
        
        /// <summary>
        /// Belirli bir yerleşim merkezini ID'ye göre getirir
        /// </summary>
        Task<Settlement?> GetSettlementByIdAsync(int id);
    }
}

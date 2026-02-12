using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    /// <summary>
    /// İl Sınırları verilerine erişim için repository interface
    /// Read-only işlemler için tasarlanmıştır
    /// </summary>
    public interface IProvinceRepository
    {
        /// <summary>
        /// Tüm il sınırlarını getirir (simplified geometry ile)
        /// </summary>
        Task<IEnumerable<Province>> GetAllProvincesAsync();
        
        /// <summary>
        /// Belirli bir il sınırını ID'ye göre getirir
        /// </summary>
        Task<Province?> GetProvinceByIdAsync(int id);
    }
}

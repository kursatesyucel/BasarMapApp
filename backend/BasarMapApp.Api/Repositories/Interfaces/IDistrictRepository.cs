using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    /// <summary>
    /// İlçe Sınırları verilerine erişim için repository interface
    /// Read-only işlemler için tasarlanmıştır
    /// </summary>
    public interface IDistrictRepository
    {
        /// <summary>
        /// Tüm ilçe sınırlarını getirir (simplified geometry ile)
        /// </summary>
        Task<IEnumerable<District>> GetAllDistrictsAsync();
        
        /// <summary>
        /// Belirli bir ilçe sınırını ID'ye göre getirir
        /// </summary>
        Task<District?> GetDistrictByIdAsync(int id);
        
        /// <summary>
        /// İsme göre ilçeleri arar
        /// </summary>
        Task<IEnumerable<District>> SearchDistrictsByNameAsync(string searchTerm);
    }
}

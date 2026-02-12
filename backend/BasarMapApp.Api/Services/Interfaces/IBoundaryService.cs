using BasarMapApp.Api.DTOs.Boundary;
using BasarMapApp.Api.Responses;

namespace BasarMapApp.Api.Services.Interfaces
{
    /// <summary>
    /// GIS referans verileri (İl, İlçe, Yerleşim) için read-only servis
    /// Yüksek performanslı harita render için optimize edilmiş
    /// </summary>
    public interface IBoundaryService
    {
        // ============= İL SINIRLARI (Province) Servisleri =============
        
        /// <summary>
        /// Tüm il sınırlarını getirir (simplified geometry ile)
        /// Sadece görsel sınır katmanı
        /// </summary>
        Task<ApiResponse<IEnumerable<ProvinceDto>>> GetAllProvincesAsync();
        
        /// <summary>
        /// Belirli bir il sınırını ID'ye göre getirir
        /// </summary>
        Task<ApiResponse<ProvinceDto>> GetProvinceByIdAsync(int id);
        
        // ============= İLÇE SINIRLARI (District) Servisleri =============
        
        /// <summary>
        /// Tüm ilçe sınırlarını getirir
        /// </summary>
        Task<ApiResponse<IEnumerable<DistrictDto>>> GetAllDistrictsAsync();
        
        /// <summary>
        /// Belirli bir ilçe sınırını ID'ye göre getirir
        /// </summary>
        Task<ApiResponse<DistrictDto>> GetDistrictByIdAsync(int id);
        
        /// <summary>
        /// İsme göre ilçe arar
        /// </summary>
        Task<ApiResponse<IEnumerable<DistrictDto>>> SearchDistrictsByNameAsync(string searchTerm);
        
        // ============= YERLEŞİM MERKEZLERİ (Settlement) Servisleri =============
        
        /// <summary>
        /// Tüm yerleşim merkezlerini getirir
        /// </summary>
        Task<ApiResponse<IEnumerable<SettlementDto>>> GetAllSettlementsAsync();
        
        /// <summary>
        /// Kategoriye göre yerleşim merkezlerini filtreler
        /// Kategoriler: BAŞKENT, İL, İLÇE
        /// </summary>
        Task<ApiResponse<IEnumerable<SettlementDto>>> GetSettlementsByCategoryAsync(string category);
        
        /// <summary>
        /// Bounding Box içindeki yerleşim merkezlerini getirir
        /// Sadece ekranda görünen alanı döndürerek performansı optimize eder
        /// </summary>
        Task<ApiResponse<IEnumerable<SettlementDto>>> GetSettlementsInBoundingBoxAsync(
            double minLon, double minLat, double maxLon, double maxLat);
        
        /// <summary>
        /// Belirli bir yerleşim merkezini ID'ye göre getirir
        /// </summary>
        Task<ApiResponse<SettlementDto>> GetSettlementByIdAsync(int id);
    }
}

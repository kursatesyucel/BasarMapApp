using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BasarMapApp.Api.Repositories.Implementations
{
    /// <summary>
    /// İl Sınırları repository implementasyonu
    /// Yüksek performanslı GIS sorguları için optimize edilmiş
    /// </summary>
    public class ProvinceRepository : IProvinceRepository
    {
        private readonly ApplicationDbContext _context;

        public ProvinceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Province>> GetAllProvincesAsync()
        {
            return await _context.Provinces
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Province?> GetProvinceByIdAsync(int id)
        {
            return await _context.Provinces
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

using BasarMapApp.Api.Data;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BasarMapApp.Api.Repositories.Implementations
{
    /// <summary>
    /// İlçe Sınırları repository implementasyonu
    /// </summary>
    public class DistrictRepository : IDistrictRepository
    {
        private readonly ApplicationDbContext _context;

        public DistrictRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<District>> GetAllDistrictsAsync()
        {
            return await _context.Districts
                .AsNoTracking()
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<District?> GetDistrictByIdAsync(int id)
        {
            return await _context.Districts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<District>> SearchDistrictsByNameAsync(string searchTerm)
        {
            return await _context.Districts
                .AsNoTracking()
                .Where(d => EF.Functions.Like(d.Name, $"%{searchTerm}%"))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
    }
}

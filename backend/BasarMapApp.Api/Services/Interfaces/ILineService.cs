using BasarMapApp.Api.DTOs.Line;
using BasarMapApp.Api.Responses;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface ILineService
    {
        Task<ApiResponse<IEnumerable<LineDto>>> GetAllAsync();
        Task<ApiResponse<LineDto>> GetByIdAsync(int id);
        Task<ApiResponse<LineDto>> CreateAsync(CreateLineDto createLineDto);
        Task<ApiResponse<LineDto>> UpdateAsync(int id, UpdateLineDto updateLineDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
} 
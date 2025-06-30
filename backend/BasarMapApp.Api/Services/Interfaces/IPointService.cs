using BasarMapApp.Api.DTOs.Point;
using BasarMapApp.Api.Responses;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IPointService
    {
        Task<ApiResponse<IEnumerable<PointDto>>> GetAllAsync();
        Task<ApiResponse<PointDto>> GetByIdAsync(int id);
        Task<ApiResponse<PointDto>> CreateAsync(CreatePointDto createPointDto);
        Task<ApiResponse<PointDto>> UpdateAsync(int id, UpdatePointDto updatePointDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
} 
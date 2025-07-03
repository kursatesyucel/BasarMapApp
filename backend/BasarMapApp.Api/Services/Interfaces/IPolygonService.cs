using BasarMapApp.Api.DTOs.Polygon;
using BasarMapApp.Api.Responses;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IPolygonService
    {
        Task<ApiResponse<IEnumerable<PolygonDto>>> GetAllAsync();
        Task<ApiResponse<PolygonDto>> GetByIdAsync(int id);
        Task<ApiResponse<PolygonDto>> CreateAsync(CreatePolygonDto createPolygonDto);
        Task<ApiResponse<PolygonDto>> CreateWithIntersectionHandlingAsync(CreatePolygonDto createPolygonDto);
        Task<ApiResponse<PolygonDto>> UpdateAsync(int id, UpdatePolygonDto updatePolygonDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
} 
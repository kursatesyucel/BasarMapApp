using BasarMapApp.Api.DTOs.Camera;
using BasarMapApp.Api.Responses;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface ICameraService
    {
        Task<ApiResponse<IEnumerable<CameraDto>>> GetAllAsync();
        Task<ApiResponse<CameraDto>> GetByIdAsync(int id);
        Task<ApiResponse<CameraDto>> CreateAsync(CreateCameraDto createCameraDto);
        Task<ApiResponse<CameraDto>> UpdateAsync(int id, UpdateCameraDto updateCameraDto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<CameraDto>>> GetActiveCamerasAsync();
        Task<ApiResponse<CameraDto>> GetByVideoFileNameAsync(string videoFileName);
    }
} 
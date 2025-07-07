using AutoMapper;
using BasarMapApp.Api.DTOs.Camera;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Services.Implementations
{
    public class CameraService : ICameraService
    {
        private readonly ICameraRepository _cameraRepository;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;

        public CameraService(ICameraRepository cameraRepository, IMapper mapper)
        {
            _cameraRepository = cameraRepository;
            _mapper = mapper;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<ApiResponse<IEnumerable<CameraDto>>> GetAllAsync()
        {
            try
            {
                var cameras = await _cameraRepository.GetAllAsync();
                var cameraDtos = _mapper.Map<IEnumerable<CameraDto>>(cameras);
                
                return ApiResponse<IEnumerable<CameraDto>>.SuccessResult(
                    cameraDtos, 
                    "Cameras retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CameraDto>>.FailureResult(
                    "An error occurred while retrieving cameras",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<CameraDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Invalid camera ID",
                        "Camera ID must be greater than 0"
                    );
                }

                var camera = await _cameraRepository.GetByIdAsync(id);
                if (camera == null)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Camera not found",
                        $"No camera found with ID: {id}"
                    );
                }

                var cameraDto = _mapper.Map<CameraDto>(camera);
                return ApiResponse<CameraDto>.SuccessResult(cameraDto, "Camera retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CameraDto>.FailureResult(
                    "An error occurred while retrieving the camera",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<CameraDto>> CreateAsync(CreateCameraDto createCameraDto)
        {
            try
            {
                // Check if video file name already exists
                var existingCamera = await _cameraRepository.GetByVideoFileNameAsync(createCameraDto.VideoFileName);
                if (existingCamera != null)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Video file name already exists",
                        $"A camera with video file name '{createCameraDto.VideoFileName}' already exists"
                    );
                }

                var camera = _mapper.Map<Camera>(createCameraDto);
                var createdCamera = await _cameraRepository.CreateAsync(camera);
                var cameraDto = _mapper.Map<CameraDto>(createdCamera);
                
                return ApiResponse<CameraDto>.SuccessResult(cameraDto, "Camera created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CameraDto>.FailureResult(
                    "An error occurred while creating the camera",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<CameraDto>> UpdateAsync(int id, UpdateCameraDto updateCameraDto)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Invalid camera ID",
                        "Camera ID must be greater than 0"
                    );
                }

                var existsResult = await _cameraRepository.ExistsAsync(id);
                if (!existsResult)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Camera not found",
                        $"No camera found with ID: {id}"
                    );
                }

                // Check if video file name already exists (excluding current camera)
                var existingCamera = await _cameraRepository.GetByVideoFileNameAsync(updateCameraDto.VideoFileName);
                if (existingCamera != null && existingCamera.Id != id)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Video file name already exists",
                        $"A camera with video file name '{updateCameraDto.VideoFileName}' already exists"
                    );
                }

                var camera = _mapper.Map<Camera>(updateCameraDto);
                var updatedCamera = await _cameraRepository.UpdateAsync(id, camera);
                
                if (updatedCamera == null)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Update failed",
                        "Camera could not be updated"
                    );
                }

                var cameraDto = _mapper.Map<CameraDto>(updatedCamera);
                return ApiResponse<CameraDto>.SuccessResult(cameraDto, "Camera updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CameraDto>.FailureResult(
                    "An error occurred while updating the camera",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<bool>.FailureResult(
                        "Invalid camera ID",
                        "Camera ID must be greater than 0"
                    );
                }

                var deleted = await _cameraRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.FailureResult(
                        "Camera not found",
                        $"No camera found with ID: {id}"
                    );
                }

                return ApiResponse<bool>.SuccessResult(true, "Camera deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult(
                    "An error occurred while deleting the camera",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<IEnumerable<CameraDto>>> GetActiveCamerasAsync()
        {
            try
            {
                var cameras = await _cameraRepository.GetActiveCamerasAsync();
                var cameraDtos = _mapper.Map<IEnumerable<CameraDto>>(cameras);
                
                return ApiResponse<IEnumerable<CameraDto>>.SuccessResult(
                    cameraDtos, 
                    "Active cameras retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CameraDto>>.FailureResult(
                    "An error occurred while retrieving active cameras",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<CameraDto>> GetByVideoFileNameAsync(string videoFileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(videoFileName))
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Invalid video file name",
                        "Video file name cannot be empty"
                    );
                }

                var camera = await _cameraRepository.GetByVideoFileNameAsync(videoFileName);
                if (camera == null)
                {
                    return ApiResponse<CameraDto>.FailureResult(
                        "Camera not found",
                        $"No camera found with video file name: {videoFileName}"
                    );
                }

                var cameraDto = _mapper.Map<CameraDto>(camera);
                return ApiResponse<CameraDto>.SuccessResult(cameraDto, "Camera retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CameraDto>.FailureResult(
                    "An error occurred while retrieving the camera",
                    ex.Message
                );
            }
        }
    }
} 
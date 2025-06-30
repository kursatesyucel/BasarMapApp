using AutoMapper;
using BasarMapApp.Api.DTOs.Point;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Services.Implementations
{
    public class PointService : IPointService
    {
        private readonly IPointRepository _pointRepository;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;

        public PointService(IPointRepository pointRepository, IMapper mapper)
        {
            _pointRepository = pointRepository;
            _mapper = mapper;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<ApiResponse<IEnumerable<PointDto>>> GetAllAsync()
        {
            try
            {
                var points = await _pointRepository.GetAllAsync();
                var pointDtos = _mapper.Map<IEnumerable<PointDto>>(points);
                
                return ApiResponse<IEnumerable<PointDto>>.SuccessResult(
                    pointDtos, 
                    "Points retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<PointDto>>.FailureResult(
                    "An error occurred while retrieving points",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PointDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<PointDto>.FailureResult(
                        "Invalid point ID",
                        "Point ID must be greater than 0"
                    );
                }

                var point = await _pointRepository.GetByIdAsync(id);
                if (point == null)
                {
                    return ApiResponse<PointDto>.FailureResult(
                        "Point not found",
                        $"No point found with ID: {id}"
                    );
                }

                var pointDto = _mapper.Map<PointDto>(point);
                return ApiResponse<PointDto>.SuccessResult(pointDto, "Point retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PointDto>.FailureResult(
                    "An error occurred while retrieving the point",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PointDto>> CreateAsync(CreatePointDto createPointDto)
        {
            try
            {
                var point = _mapper.Map<MapPoint>(createPointDto);
                var createdPoint = await _pointRepository.CreateAsync(point);
                var pointDto = _mapper.Map<PointDto>(createdPoint);
                
                return ApiResponse<PointDto>.SuccessResult(pointDto, "Point created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PointDto>.FailureResult(
                    "An error occurred while creating the point",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PointDto>> UpdateAsync(int id, UpdatePointDto updatePointDto)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<PointDto>.FailureResult(
                        "Invalid point ID",
                        "Point ID must be greater than 0"
                    );
                }

                var existsResult = await _pointRepository.ExistsAsync(id);
                if (!existsResult)
                {
                    return ApiResponse<PointDto>.FailureResult(
                        "Point not found",
                        $"No point found with ID: {id}"
                    );
                }

                var point = _mapper.Map<MapPoint>(updatePointDto);
                var updatedPoint = await _pointRepository.UpdateAsync(id, point);
                
                if (updatedPoint == null)
                {
                    return ApiResponse<PointDto>.FailureResult(
                        "Update failed",
                        "Point could not be updated"
                    );
                }

                var pointDto = _mapper.Map<PointDto>(updatedPoint);
                return ApiResponse<PointDto>.SuccessResult(pointDto, "Point updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PointDto>.FailureResult(
                    "An error occurred while updating the point",
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
                        "Invalid point ID",
                        "Point ID must be greater than 0"
                    );
                }

                var deleted = await _pointRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.FailureResult(
                        "Point not found",
                        $"No point found with ID: {id}"
                    );
                }

                return ApiResponse<bool>.SuccessResult(true, "Point deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult(
                    "An error occurred while deleting the point",
                    ex.Message
                );
            }
        }
    }
} 
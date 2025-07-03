using AutoMapper;
using BasarMapApp.Api.DTOs.Polygon;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Services.Implementations
{
    public class PolygonService : IPolygonService
    {
        private readonly IPolygonRepository _polygonRepository;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;

        public PolygonService(IPolygonRepository polygonRepository, IMapper mapper)
        {
            _polygonRepository = polygonRepository;
            _mapper = mapper;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<ApiResponse<IEnumerable<PolygonDto>>> GetAllAsync()
        {
            try
            {
                var polygons = await _polygonRepository.GetAllAsync();
                var polygonDtos = _mapper.Map<IEnumerable<PolygonDto>>(polygons);
                
                return ApiResponse<IEnumerable<PolygonDto>>.SuccessResult(
                    polygonDtos, 
                    "Polygons retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<PolygonDto>>.FailureResult(
                    "An error occurred while retrieving polygons",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PolygonDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<PolygonDto>.FailureResult(
                        "Invalid polygon ID",
                        "Polygon ID must be greater than 0"
                    );
                }

                var polygon = await _polygonRepository.GetByIdAsync(id);
                if (polygon == null)
                {
                    return ApiResponse<PolygonDto>.FailureResult(
                        "Polygon not found",
                        $"No polygon found with ID: {id}"
                    );
                }

                var polygonDto = _mapper.Map<PolygonDto>(polygon);
                return ApiResponse<PolygonDto>.SuccessResult(polygonDto, "Polygon retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PolygonDto>.FailureResult(
                    "An error occurred while retrieving the polygon",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PolygonDto>> CreateAsync(CreatePolygonDto createPolygonDto)
        {
            try
            {
                var polygon = _mapper.Map<MapPolygon>(createPolygonDto);
                var createdPolygon = await _polygonRepository.CreateAsync(polygon);
                var polygonDto = _mapper.Map<PolygonDto>(createdPolygon);
                
                return ApiResponse<PolygonDto>.SuccessResult(polygonDto, "Polygon created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PolygonDto>.FailureResult(
                    "An error occurred while creating the polygon",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PolygonDto>> CreateWithIntersectionHandlingAsync(CreatePolygonDto createPolygonDto)
        {
            try
            {
                // Convert DTO to entity
                var polygonEntity = _mapper.Map<MapPolygon>(createPolygonDto);
                var newPolygon = polygonEntity.Geometry;

                // Check for intersections and calculate difference
                var resultPolygon = await _polygonRepository.GetDifferenceFromExistingPolygonsAsync(newPolygon);

                if (resultPolygon == null)
                {
                    return ApiResponse<PolygonDto>.FailureResult(
                        "Polygon creation failed",
                        "The new polygon is completely contained within existing polygons or resulted in an empty geometry."
                    );
                }

                // Create new entity with the difference geometry
                var polygonToSave = new MapPolygon
                {
                    Name = createPolygonDto.Name,
                    Description = createPolygonDto.Description,
                    Geometry = resultPolygon
                };

                var createdPolygon = await _polygonRepository.CreateAsync(polygonToSave);
                var polygonDto = _mapper.Map<PolygonDto>(createdPolygon);
                
                // Check if the result is different from the original request
                var originalArea = newPolygon.Area;
                var resultArea = resultPolygon.Area;
                var message = originalArea > resultArea 
                    ? $"Polygon created successfully. Note: {((originalArea - resultArea) / originalArea * 100):F1}% of the original area was removed due to intersection with existing polygons."
                    : "Polygon created successfully";

                return ApiResponse<PolygonDto>.SuccessResult(polygonDto, message);
            }
            catch (Exception ex)
            {
                return ApiResponse<PolygonDto>.FailureResult(
                    "An error occurred while creating the polygon with intersection handling",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<PolygonDto>> UpdateAsync(int id, UpdatePolygonDto updatePolygonDto)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<PolygonDto>.FailureResult(
                        "Invalid polygon ID",
                        "Polygon ID must be greater than 0"
                    );
                }

                var existsResult = await _polygonRepository.ExistsAsync(id);
                if (!existsResult)
                {
                    return ApiResponse<PolygonDto>.FailureResult(
                        "Polygon not found",
                        $"No polygon found with ID: {id}"
                    );
                }

                var polygon = _mapper.Map<MapPolygon>(updatePolygonDto);
                var updatedPolygon = await _polygonRepository.UpdateAsync(id, polygon);
                
                if (updatedPolygon == null)
                {
                    return ApiResponse<PolygonDto>.FailureResult(
                        "Update failed",
                        "Polygon could not be updated"
                    );
                }

                var polygonDto = _mapper.Map<PolygonDto>(updatedPolygon);
                return ApiResponse<PolygonDto>.SuccessResult(polygonDto, "Polygon updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PolygonDto>.FailureResult(
                    "An error occurred while updating the polygon",
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
                        "Invalid polygon ID",
                        "Polygon ID must be greater than 0"
                    );
                }

                var deleted = await _polygonRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.FailureResult(
                        "Polygon not found",
                        $"No polygon found with ID: {id}"
                    );
                }

                return ApiResponse<bool>.SuccessResult(true, "Polygon deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult(
                    "An error occurred while deleting the polygon",
                    ex.Message
                );
            }
        }
    }
} 
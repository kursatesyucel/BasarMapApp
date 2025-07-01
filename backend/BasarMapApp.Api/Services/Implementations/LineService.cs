using AutoMapper;
using BasarMapApp.Api.DTOs.Line;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace BasarMapApp.Api.Services.Implementations
{
    public class LineService : ILineService
    {
        private readonly ILineRepository _lineRepository;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;

        public LineService(ILineRepository lineRepository, IMapper mapper)
        {
            _lineRepository = lineRepository;
            _mapper = mapper;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<ApiResponse<IEnumerable<LineDto>>> GetAllAsync()
        {
            try
            {
                var lines = await _lineRepository.GetAllAsync();
                var lineDtos = _mapper.Map<IEnumerable<LineDto>>(lines);
                
                return ApiResponse<IEnumerable<LineDto>>.SuccessResult(
                    lineDtos, 
                    "Lines retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<LineDto>>.FailureResult(
                    "An error occurred while retrieving lines",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<LineDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<LineDto>.FailureResult(
                        "Invalid line ID",
                        "Line ID must be greater than 0"
                    );
                }

                var line = await _lineRepository.GetByIdAsync(id);
                if (line == null)
                {
                    return ApiResponse<LineDto>.FailureResult(
                        "Line not found",
                        $"No line found with ID: {id}"
                    );
                }

                var lineDto = _mapper.Map<LineDto>(line);
                return ApiResponse<LineDto>.SuccessResult(lineDto, "Line retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LineDto>.FailureResult(
                    "An error occurred while retrieving the line",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<LineDto>> CreateAsync(CreateLineDto createLineDto)
        {
            try
            {
                var line = _mapper.Map<MapLine>(createLineDto);
                var createdLine = await _lineRepository.CreateAsync(line);
                var lineDto = _mapper.Map<LineDto>(createdLine);
                
                return ApiResponse<LineDto>.SuccessResult(lineDto, "Line created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LineDto>.FailureResult(
                    "An error occurred while creating the line",
                    ex.Message
                );
            }
        }

        public async Task<ApiResponse<LineDto>> UpdateAsync(int id, UpdateLineDto updateLineDto)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<LineDto>.FailureResult(
                        "Invalid line ID",
                        "Line ID must be greater than 0"
                    );
                }

                var existsResult = await _lineRepository.ExistsAsync(id);
                if (!existsResult)
                {
                    return ApiResponse<LineDto>.FailureResult(
                        "Line not found",
                        $"No line found with ID: {id}"
                    );
                }

                var line = _mapper.Map<MapLine>(updateLineDto);
                var updatedLine = await _lineRepository.UpdateAsync(id, line);
                
                if (updatedLine == null)
                {
                    return ApiResponse<LineDto>.FailureResult(
                        "Update failed",
                        "Line could not be updated"
                    );
                }

                var lineDto = _mapper.Map<LineDto>(updatedLine);
                return ApiResponse<LineDto>.SuccessResult(lineDto, "Line updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<LineDto>.FailureResult(
                    "An error occurred while updating the line",
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
                        "Invalid line ID",
                        "Line ID must be greater than 0"
                    );
                }

                var deleted = await _lineRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return ApiResponse<bool>.FailureResult(
                        "Line not found",
                        $"No line found with ID: {id}"
                    );
                }

                return ApiResponse<bool>.SuccessResult(true, "Line deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult(
                    "An error occurred while deleting the line",
                    ex.Message
                );
            }
        }
    }
} 
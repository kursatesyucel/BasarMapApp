using BasarMapApp.Api.DTOs.Point;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointsController : ControllerBase
    {
        private readonly IPointService _pointService;

        public PointsController(IPointService pointService)
        {
            _pointService = pointService;
        }

        /// <summary>
        /// Get all points
        /// </summary>
        /// <returns>List of all points</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PointDto>>>> GetAllPoints()
        {
            var result = await _pointService.GetAllAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get a point by ID
        /// </summary>
        /// <param name="id">Point ID</param>
        /// <returns>Point with the specified ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PointDto>>> GetPoint(int id)
        {
            var result = await _pointService.GetByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Create a new point
        /// </summary>
        /// <param name="createPointDto">Point creation data</param>
        /// <returns>Created point</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PointDto>>> CreatePoint([FromBody] CreatePointDto createPointDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<PointDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _pointService.CreateAsync(createPointDto);
            
            if (result.Success)
                return CreatedAtAction(
                    nameof(GetPoint), 
                    new { id = result.Data!.Id }, 
                    result
                );
            
            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing point
        /// </summary>
        /// <param name="id">Point ID</param>
        /// <param name="updatePointDto">Point update data</param>
        /// <returns>Updated point</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PointDto>>> UpdatePoint(int id, [FromBody] UpdatePointDto updatePointDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<PointDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _pointService.UpdateAsync(id, updatePointDto);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Delete a point
        /// </summary>
        /// <param name="id">Point ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePoint(int id)
        {
            var result = await _pointService.DeleteAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }
    }
} 
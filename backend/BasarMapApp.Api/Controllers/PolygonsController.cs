using BasarMapApp.Api.DTOs.Polygon;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PolygonsController : ControllerBase
    {
        private readonly IPolygonService _polygonService;

        public PolygonsController(IPolygonService polygonService)
        {
            _polygonService = polygonService;
        }

        /// <summary>
        /// Get all polygons
        /// </summary>
        /// <returns>List of all polygons</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PolygonDto>>>> GetAllPolygons()
        {
            var result = await _polygonService.GetAllAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get a polygon by ID
        /// </summary>
        /// <param name="id">Polygon ID</param>
        /// <returns>Polygon with the specified ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PolygonDto>>> GetPolygon(int id)
        {
            var result = await _polygonService.GetByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Create a new polygon
        /// </summary>
        /// <param name="createPolygonDto">Polygon creation data</param>
        /// <returns>Created polygon</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<PolygonDto>>> CreatePolygon([FromBody] CreatePolygonDto createPolygonDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<PolygonDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _polygonService.CreateAsync(createPolygonDto);
            
            if (result.Success)
                return CreatedAtAction(
                    nameof(GetPolygon), 
                    new { id = result.Data!.Id }, 
                    result
                );
            
            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing polygon
        /// </summary>
        /// <param name="id">Polygon ID</param>
        /// <param name="updatePolygonDto">Polygon update data</param>
        /// <returns>Updated polygon</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<PolygonDto>>> UpdatePolygon(int id, [FromBody] UpdatePolygonDto updatePolygonDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<PolygonDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _polygonService.UpdateAsync(id, updatePolygonDto);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Delete a polygon
        /// </summary>
        /// <param name="id">Polygon ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePolygon(int id)
        {
            var result = await _polygonService.DeleteAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }
    }
} 
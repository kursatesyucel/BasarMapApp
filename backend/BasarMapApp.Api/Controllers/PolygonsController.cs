using BasarMapApp.Api.DTOs.Polygon;
using BasarMapApp.Api.Filters;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All authenticated users can view
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
        /// Create a new polygon - Admin only
        /// </summary>
        /// <param name="createPolygonDto">Polygon creation data</param>
        /// <returns>Created polygon</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "Polygon", action: "Create")]
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
        /// Create a new polygon with intersection handling (removes overlaps with existing polygons) - Admin only
        /// </summary>
        /// <param name="createPolygonDto">Polygon creation data</param>
        /// <returns>Created polygon with intersections removed</returns>
        [HttpPost("create-with-intersection-handling")]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "Polygon", action: "Create")]
        public async Task<ActionResult<ApiResponse<PolygonDto>>> CreatePolygonWithIntersectionHandling([FromBody] CreatePolygonDto createPolygonDto)
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

            var result = await _polygonService.CreateWithIntersectionHandlingAsync(createPolygonDto);
            
            if (result.Success)
                return CreatedAtAction(
                    nameof(GetPolygon), 
                    new { id = result.Data!.Id }, 
                    result
                );
            
            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing polygon - Admin only
        /// </summary>
        /// <param name="id">Polygon ID</param>
        /// <param name="updatePolygonDto">Polygon update data</param>
        /// <returns>Updated polygon</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "Polygon", action: "Update")]
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
        /// Delete a polygon - Admin only
        /// </summary>
        /// <param name="id">Polygon ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "Polygon", action: "Delete")]
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
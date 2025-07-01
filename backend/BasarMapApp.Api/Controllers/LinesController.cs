using BasarMapApp.Api.DTOs.Line;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinesController : ControllerBase
    {
        private readonly ILineService _lineService;

        public LinesController(ILineService lineService)
        {
            _lineService = lineService;
        }

        /// <summary>
        /// Get all lines
        /// </summary>
        /// <returns>List of all lines</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<LineDto>>>> GetAllLines()
        {
            var result = await _lineService.GetAllAsync();
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Get a line by ID
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <returns>Line with the specified ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LineDto>>> GetLine(int id)
        {
            var result = await _lineService.GetByIdAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Create a new line
        /// </summary>
        /// <param name="createLineDto">Line creation data</param>
        /// <returns>Created line</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<LineDto>>> CreateLine([FromBody] CreateLineDto createLineDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<LineDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _lineService.CreateAsync(createLineDto);
            
            if (result.Success)
                return CreatedAtAction(
                    nameof(GetLine), 
                    new { id = result.Data!.Id }, 
                    result
                );
            
            return BadRequest(result);
        }

        /// <summary>
        /// Update an existing line
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <param name="updateLineDto">Line update data</param>
        /// <returns>Updated line</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<LineDto>>> UpdateLine(int id, [FromBody] UpdateLineDto updateLineDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var validationResult = ApiResponse<LineDto>.FailureResult(
                    "Validation failed", 
                    errors
                );
                
                return BadRequest(validationResult);
            }

            var result = await _lineService.UpdateAsync(id, updateLineDto);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Delete a line
        /// </summary>
        /// <param name="id">Line ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteLine(int id)
        {
            var result = await _lineService.DeleteAsync(id);
            
            if (result.Success)
                return Ok(result);
            
            if (result.Message.Contains("not found"))
                return NotFound(result);
            
            return BadRequest(result);
        }
    }
} 
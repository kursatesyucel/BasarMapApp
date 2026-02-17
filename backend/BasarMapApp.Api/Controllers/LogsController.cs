using BasarMapApp.Api.DTOs.Logs;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    /// <summary>
    /// Admin-only API for viewing MongoDB logs (Login and Audit)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Get paginated login logs with optional filters
        /// </summary>
        /// <param name="filter">Pagination and filter parameters (from query string)</param>
        /// <returns>Paginated login logs</returns>
        [HttpGet("login")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedLogResult<LoginLogDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedLogResult<LoginLogDto>>>> GetLoginLogs([FromQuery] LogFilterDto filter)
        {
            var result = await _logService.GetLoginLogsAsync(filter);
            return Ok(ApiResponse<PaginatedLogResult<LoginLogDto>>.SuccessResult(result, "Login logs retrieved successfully"));
        }

        /// <summary>
        /// Get paginated audit logs with optional filters
        /// </summary>
        /// <param name="filter">Pagination and filter parameters (from query string)</param>
        /// <returns>Paginated audit logs</returns>
        [HttpGet("audit")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedLogResult<AuditLogDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PaginatedLogResult<AuditLogDto>>>> GetAuditLogs([FromQuery] LogFilterDto filter)
        {
            var result = await _logService.GetAuditLogsAsync(filter);
            return Ok(ApiResponse<PaginatedLogResult<AuditLogDto>>.SuccessResult(result, "Audit logs retrieved successfully"));
        }

        /// <summary>
        /// Get combined login and audit logs for a specific user
        /// </summary>
        /// <param name="userId">User ID to filter by</param>
        /// <param name="filter">Pagination and filter parameters (from query string)</param>
        /// <returns>User's login and audit log history</returns>
        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserLogsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserLogsResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserLogsResponseDto>>> GetUserLogs(int userId, [FromQuery] LogFilterDto filter)
        {
            if (userId <= 0)
            {
                return BadRequest(ApiResponse<UserLogsResponseDto>.FailureResult("Invalid user ID"));
            }

            var result = await _logService.GetUserLogsAsync(userId, filter);
            return Ok(ApiResponse<UserLogsResponseDto>.SuccessResult(result, "User logs retrieved successfully"));
        }
    }
}

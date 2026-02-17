using BasarMapApp.Api.DTOs.Logs;
using BasarMapApp.Api.Models.Logs;

namespace BasarMapApp.Api.Services.Interfaces
{
    /// <summary>
    /// Service for managing application logs stored in MongoDB
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Creates a login log entry asynchronously (fire-and-forget pattern)
        /// </summary>
        Task CreateLoginLogAsync(LoginLog loginLog);

        /// <summary>
        /// Creates an audit log entry asynchronously (fire-and-forget pattern)
        /// </summary>
        Task CreateAuditLogAsync(AuditLog auditLog);

        /// <summary>
        /// Gets paginated login logs with optional filters
        /// </summary>
        /// <param name="filter">Pagination and filter parameters</param>
        /// <returns>Paginated login logs</returns>
        Task<PaginatedLogResult<LoginLogDto>> GetLoginLogsAsync(LogFilterDto filter);

        /// <summary>
        /// Gets paginated audit logs with optional filters
        /// </summary>
        /// <param name="filter">Pagination and filter parameters</param>
        /// <returns>Paginated audit logs</returns>
        Task<PaginatedLogResult<AuditLogDto>> GetAuditLogsAsync(LogFilterDto filter);

        /// <summary>
        /// Gets both login and audit logs for a specific user
        /// </summary>
        /// <param name="userId">User ID to filter by</param>
        /// <param name="filter">Pagination and filter parameters</param>
        /// <returns>Combined login and audit logs for the user</returns>
        Task<UserLogsResponseDto> GetUserLogsAsync(int userId, LogFilterDto filter);
    }
}

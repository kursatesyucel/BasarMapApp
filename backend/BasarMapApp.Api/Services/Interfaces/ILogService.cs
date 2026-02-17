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
        /// <param name="loginLog">Login log details</param>
        /// <returns>Task that completes when log is queued (not necessarily written)</returns>
        Task CreateLoginLogAsync(LoginLog loginLog);

        /// <summary>
        /// Creates an audit log entry asynchronously (fire-and-forget pattern)
        /// </summary>
        /// <param name="auditLog">Audit log details</param>
        /// <returns>Task that completes when log is queued (not necessarily written)</returns>
        Task CreateAuditLogAsync(AuditLog auditLog);
    }
}

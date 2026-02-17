using BasarMapApp.Api.Configuration;
using BasarMapApp.Api.Models.Logs;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BasarMapApp.Api.Services.Implementations
{
    /// <summary>
    /// Service for managing application logs in MongoDB.
    /// Implements fire-and-forget pattern to avoid blocking main application threads.
    /// </summary>
    public class LogService : ILogService
    {
        private readonly IMongoCollection<LoginLog> _loginLogsCollection;
        private readonly IMongoCollection<AuditLog> _auditLogsCollection;
        private readonly ILogger<LogService> _logger;

        public LogService(IOptions<MongoDbSettings> mongoSettings, ILogger<LogService> logger)
        {
            _logger = logger;

            try
            {
                var settings = mongoSettings.Value;
                
                // Validate configuration
                if (string.IsNullOrEmpty(settings.ConnectionString))
                {
                    _logger.LogError("MongoDB ConnectionString is not configured");
                    throw new InvalidOperationException("MongoDB ConnectionString is not configured");
                }

                if (string.IsNullOrEmpty(settings.DatabaseName))
                {
                    _logger.LogError("MongoDB DatabaseName is not configured");
                    throw new InvalidOperationException("MongoDB DatabaseName is not configured");
                }

                // Create MongoDB client
                var client = new MongoClient(settings.ConnectionString);
                var database = client.GetDatabase(settings.DatabaseName);

                // Initialize collections
                _loginLogsCollection = database.GetCollection<LoginLog>(settings.LoginLogsCollectionName);
                _auditLogsCollection = database.GetCollection<AuditLog>(settings.AuditLogsCollectionName);

                _logger.LogInformation("LogService initialized successfully with database: {DatabaseName}", settings.DatabaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize LogService. Logging functionality will be disabled.");
                // Re-throw to prevent service registration if MongoDB is not properly configured
                throw;
            }
        }

        /// <summary>
        /// Creates a login log entry asynchronously using fire-and-forget pattern.
        /// Errors are caught and logged but do not affect the main application flow.
        /// </summary>
        public async Task CreateLoginLogAsync(LoginLog loginLog)
        {
            // Fire-and-forget: Don't await this task in the calling code
            _ = Task.Run(async () =>
            {
                try
                {
                    // Ensure timestamp is UTC
                    if (loginLog.Timestamp.Kind != DateTimeKind.Utc)
                    {
                        loginLog.Timestamp = loginLog.Timestamp.ToUniversalTime();
                    }

                    await _loginLogsCollection.InsertOneAsync(loginLog);
                    
                    _logger.LogDebug("Login log created for email: {Email}, Success: {IsSuccess}", 
                        loginLog.Email, loginLog.IsSuccess);
                }
                catch (Exception ex)
                {
                    // Swallow the exception - logging should never break the main application
                    _logger.LogError(ex, "Failed to create login log for email: {Email}. Logging error is ignored.", 
                        loginLog.Email);
                }
            });

            // Return immediately without waiting for the log to be written
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates an audit log entry asynchronously using fire-and-forget pattern.
        /// Errors are caught and logged but do not affect the main application flow.
        /// </summary>
        public async Task CreateAuditLogAsync(AuditLog auditLog)
        {
            // Fire-and-forget: Don't await this task in the calling code
            _ = Task.Run(async () =>
            {
                try
                {
                    // Ensure timestamp is UTC
                    if (auditLog.Timestamp.Kind != DateTimeKind.Utc)
                    {
                        auditLog.Timestamp = auditLog.Timestamp.ToUniversalTime();
                    }

                    await _auditLogsCollection.InsertOneAsync(auditLog);
                    
                    _logger.LogDebug("Audit log created: {Action} on {EntityName} (ID: {EntityId}) by User {UserId}", 
                        auditLog.Action, auditLog.EntityName, auditLog.EntityId, auditLog.UserId);
                }
                catch (Exception ex)
                {
                    // Swallow the exception - logging should never break the main application
                    _logger.LogError(ex, "Failed to create audit log for {Action} on {EntityName}. Logging error is ignored.", 
                        auditLog.Action, auditLog.EntityName);
                }
            });

            // Return immediately without waiting for the log to be written
            await Task.CompletedTask;
        }
    }
}

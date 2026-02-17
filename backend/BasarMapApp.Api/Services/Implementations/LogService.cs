using BasarMapApp.Api.Configuration;
using BasarMapApp.Api.DTOs.Logs;
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

        /// <summary>
        /// Gets paginated login logs with database-level filtering
        /// </summary>
        public async Task<PaginatedLogResult<LoginLogDto>> GetLoginLogsAsync(LogFilterDto filter)
        {
            var builder = Builders<LoginLog>.Filter;
            var filters = new List<FilterDefinition<LoginLog>>();

            // Date range filter
            if (filter.StartDate.HasValue)
            {
                filters.Add(builder.Gte(x => x.Timestamp, filter.StartDate.Value.ToUniversalTime()));
            }
            if (filter.EndDate.HasValue)
            {
                var endDate = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                filters.Add(builder.Lte(x => x.Timestamp, endDate.ToUniversalTime()));
            }

            // UserId filter
            if (!string.IsNullOrEmpty(filter.UserId) && int.TryParse(filter.UserId, out int userId))
            {
                filters.Add(builder.Eq(x => x.UserId, userId));
            }

            // IsSuccess filter (Login logs only)
            if (filter.IsSuccess.HasValue)
            {
                filters.Add(builder.Eq(x => x.IsSuccess, filter.IsSuccess.Value));
            }

            // SearchText: Email or IpAddress
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                filters.Add(builder.Or(
                    builder.Regex(x => x.Email, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    builder.Regex(x => x.IpAddress, new MongoDB.Bson.BsonRegularExpression(search, "i"))
                ));
            }

            var filterDef = filters.Count > 0 ? builder.And(filters) : builder.Empty;
            var sort = Builders<LoginLog>.Sort.Descending(x => x.Timestamp);

            var skip = (Math.Max(1, filter.Page) - 1) * Math.Max(1, Math.Min(filter.PageSize, 100));
            var limit = Math.Max(1, Math.Min(filter.PageSize, 100));

            var totalCount = await _loginLogsCollection.CountDocumentsAsync(filterDef);
            var items = await _loginLogsCollection
                .Find(filterDef)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();

            return new PaginatedLogResult<LoginLogDto>
            {
                Items = items.Select(MapToLoginLogDto).ToList(),
                Page = Math.Max(1, filter.Page),
                PageSize = limit,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Gets paginated audit logs with database-level filtering
        /// </summary>
        public async Task<PaginatedLogResult<AuditLogDto>> GetAuditLogsAsync(LogFilterDto filter)
        {
            var builder = Builders<AuditLog>.Filter;
            var filters = new List<FilterDefinition<AuditLog>>();

            // Date range filter
            if (filter.StartDate.HasValue)
            {
                filters.Add(builder.Gte(x => x.Timestamp, filter.StartDate.Value.ToUniversalTime()));
            }
            if (filter.EndDate.HasValue)
            {
                var endDate = filter.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                filters.Add(builder.Lte(x => x.Timestamp, endDate.ToUniversalTime()));
            }

            // UserId filter
            if (!string.IsNullOrEmpty(filter.UserId) && int.TryParse(filter.UserId, out int userId))
            {
                filters.Add(builder.Eq(x => x.UserId, userId));
            }

            // Action filter
            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                filters.Add(builder.Eq(x => x.Action, filter.Action.Trim()));
            }

            // EntityName filter
            if (!string.IsNullOrWhiteSpace(filter.EntityName))
            {
                filters.Add(builder.Eq(x => x.EntityName, filter.EntityName.Trim()));
            }

            // SearchText: Action, EntityName, EntityId
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                filters.Add(builder.Or(
                    builder.Regex(x => x.Action, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    builder.Regex(x => x.EntityName, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    builder.Regex(x => x.EntityId, new MongoDB.Bson.BsonRegularExpression(search, "i"))
                ));
            }

            var filterDef = filters.Count > 0 ? builder.And(filters) : builder.Empty;
            var sort = Builders<AuditLog>.Sort.Descending(x => x.Timestamp);

            var skip = (Math.Max(1, filter.Page) - 1) * Math.Max(1, Math.Min(filter.PageSize, 100));
            var limit = Math.Max(1, Math.Min(filter.PageSize, 100));

            var totalCount = await _auditLogsCollection.CountDocumentsAsync(filterDef);
            var items = await _auditLogsCollection
                .Find(filterDef)
                .Sort(sort)
                .Skip(skip)
                .Limit(limit)
                .ToListAsync();

            return new PaginatedLogResult<AuditLogDto>
            {
                Items = items.Select(MapToAuditLogDto).ToList(),
                Page = Math.Max(1, filter.Page),
                PageSize = limit,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Gets combined login and audit logs for a specific user
        /// </summary>
        public async Task<UserLogsResponseDto> GetUserLogsAsync(int userId, LogFilterDto filter)
        {
            var userFilter = new LogFilterDto
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                StartDate = filter.StartDate,
                EndDate = filter.EndDate,
                UserId = userId.ToString(),
                IsSuccess = filter.IsSuccess,
                SearchText = filter.SearchText,
                Action = filter.Action,
                EntityName = filter.EntityName
            };

            var loginLogs = await GetLoginLogsAsync(userFilter);
            var auditLogs = await GetAuditLogsAsync(userFilter);

            return new UserLogsResponseDto
            {
                LoginLogs = loginLogs,
                AuditLogs = auditLogs
            };
        }

        private static LoginLogDto MapToLoginLogDto(LoginLog log) => new()
        {
            Id = log.Id,
            UserId = log.UserId,
            Email = log.Email,
            Timestamp = log.Timestamp,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            IsSuccess = log.IsSuccess,
            FailureReason = log.FailureReason
        };

        private static AuditLogDto MapToAuditLogDto(AuditLog log) => new()
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            EntityName = log.EntityName,
            EntityId = log.EntityId,
            Timestamp = log.Timestamp,
            IpAddress = log.IpAddress,
            NewValues = log.NewValues
        };
    }
}

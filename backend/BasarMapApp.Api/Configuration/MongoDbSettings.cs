namespace BasarMapApp.Api.Configuration
{
    /// <summary>
    /// MongoDB connection settings read from appsettings.json
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// MongoDB connection string (e.g., "mongodb://localhost:27017")
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Database name for logging
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// Collection name for login logs
        /// </summary>
        public string LoginLogsCollectionName { get; set; } = "LoginLogs";

        /// <summary>
        /// Collection name for audit logs
        /// </summary>
        public string AuditLogsCollectionName { get; set; } = "AuditLogs";
    }
}

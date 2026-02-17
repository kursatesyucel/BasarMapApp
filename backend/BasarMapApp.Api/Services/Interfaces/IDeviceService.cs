using BasarMapApp.Api.Models.Security;

namespace BasarMapApp.Api.Services.Interfaces
{
    /// <summary>
    /// Service for managing user device tracking and session security
    /// </summary>
    public interface IDeviceService
    {
        /// <summary>
        /// Tracks a device login attempt. Updates existing device or creates new one.
        /// Sends email alert for new devices.
        /// </summary>
        /// <param name="userId">User ID from PostgreSQL</param>
        /// <param name="deviceId">Unique device identifier from frontend</param>
        /// <param name="userAgent">User-Agent string from HTTP request</param>
        /// <param name="ipAddress">IP address of the device</param>
        Task TrackDeviceAsync(int userId, string deviceId, string userAgent, string ipAddress);

        /// <summary>
        /// Gets all devices for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user devices ordered by last login date (newest first)</returns>
        Task<IEnumerable<UserDevice>> GetUserDevicesAsync(int userId);

        /// <summary>
        /// Revokes (deletes) a device for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="deviceId">Device ID to revoke</param>
        /// <returns>True if device was found and deleted, false otherwise</returns>
        Task<bool> RevokeDeviceAsync(int userId, string deviceId);
    }
}

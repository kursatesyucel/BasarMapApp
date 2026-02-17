using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BasarMapApp.Api.Models.Security
{
    /// <summary>
    /// Represents a user's device for session and security tracking stored in MongoDB
    /// </summary>
    public class UserDevice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// User ID from the main database (PostgreSQL)
        /// </summary>
        [BsonElement("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Unique device identifier (hash from frontend)
        /// </summary>
        [BsonElement("deviceId")]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable device name (e.g., "Chrome on Windows 10")
        /// </summary>
        [BsonElement("deviceName")]
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// IP Address of the device
        /// </summary>
        [BsonElement("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Last login date from this device (UTC)
        /// </summary>
        [BsonElement("lastLoginDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this device is trusted
        /// </summary>
        [BsonElement("isTrusted")]
        public bool IsTrusted { get; set; } = true;

        /// <summary>
        /// First seen date (device registration date)
        /// </summary>
        [BsonElement("firstSeenDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FirstSeenDate { get; set; } = DateTime.UtcNow;
    }
}

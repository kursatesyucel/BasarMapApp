using BasarMapApp.Api.Models.Security;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Services.Interfaces;
using MongoDB.Driver;

namespace BasarMapApp.Api.Services.Implementations
{
    public class DeviceService : IDeviceService
    {
        private readonly IMongoCollection<UserDevice> _devicesCollection;
        private readonly IMailService _mailService;
        private readonly ILogger<DeviceService> _logger;
        private readonly IUserRepository _userRepository;

        public DeviceService(
            IConfiguration configuration,
            IMailService mailService,
            ILogger<DeviceService> logger,
            IUserRepository userRepository)
        {
            var mongoSettings = configuration.GetSection("MongoDbSettings");
            var connectionString = mongoSettings["ConnectionString"];
            var databaseName = mongoSettings["DatabaseName"];
            var collectionName = mongoSettings["UserDevicesCollectionName"];

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(collectionName))
            {
                throw new InvalidOperationException("MongoDB settings for UserDevices are not configured properly");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _devicesCollection = database.GetCollection<UserDevice>(collectionName);
            _mailService = mailService;
            _logger = logger;
            _userRepository = userRepository;

            // Create index on userId and deviceId for fast lookups
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var userIdIndexModel = new CreateIndexModel<UserDevice>(
                    Builders<UserDevice>.IndexKeys.Ascending(d => d.UserId)
                );

                var deviceIdIndexModel = new CreateIndexModel<UserDevice>(
                    Builders<UserDevice>.IndexKeys.Ascending(d => d.DeviceId)
                );

                var compoundIndexModel = new CreateIndexModel<UserDevice>(
                    Builders<UserDevice>.IndexKeys
                        .Ascending(d => d.UserId)
                        .Ascending(d => d.DeviceId)
                );

                _devicesCollection.Indexes.CreateMany(new[]
                {
                    userIdIndexModel,
                    deviceIdIndexModel,
                    compoundIndexModel
                });

                _logger.LogInformation("UserDevices collection indexes created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create indexes for UserDevices collection. This is not critical.");
            }
        }

        public async Task TrackDeviceAsync(int userId, string deviceId, string userAgent, string ipAddress)
        {
            try
            {
                // Check if device already exists
                var filter = Builders<UserDevice>.Filter.And(
                    Builders<UserDevice>.Filter.Eq(d => d.UserId, userId),
                    Builders<UserDevice>.Filter.Eq(d => d.DeviceId, deviceId)
                );

                var existingDevice = await _devicesCollection.Find(filter).FirstOrDefaultAsync();

                if (existingDevice != null)
                {
                    // Update existing device
                    var update = Builders<UserDevice>.Update
                        .Set(d => d.LastLoginDate, DateTime.UtcNow)
                        .Set(d => d.IpAddress, ipAddress);

                    await _devicesCollection.UpdateOneAsync(filter, update);
                    _logger.LogInformation("Updated device {DeviceId} for user {UserId}", deviceId, userId);
                }
                else
                {
                    // New device - create record
                    var deviceName = ParseUserAgent(userAgent);
                    var newDevice = new UserDevice
                    {
                        UserId = userId,
                        DeviceId = deviceId,
                        DeviceName = deviceName,
                        IpAddress = ipAddress,
                        LastLoginDate = DateTime.UtcNow,
                        FirstSeenDate = DateTime.UtcNow,
                        IsTrusted = true
                    };

                    await _devicesCollection.InsertOneAsync(newDevice);
                    _logger.LogInformation("New device {DeviceId} registered for user {UserId}", deviceId, userId);

                    // Send email alert for new device (fire-and-forget)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var user = await _userRepository.GetByIdAsync(userId);
                            if (user != null)
                            {
                                await _mailService.SendNewDeviceAlertAsync(
                                    user.Email,
                                    user.Username,
                                    deviceName,
                                    ipAddress
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send new device alert email for user {UserId}", userId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Device tracking should never break the login flow
                _logger.LogError(ex, "Error tracking device for user {UserId}. Device tracking failed but login continues.", userId);
            }
        }

        public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(int userId)
        {
            try
            {
                var filter = Builders<UserDevice>.Filter.Eq(d => d.UserId, userId);
                var sort = Builders<UserDevice>.Sort.Descending(d => d.LastLoginDate);

                return await _devicesCollection
                    .Find(filter)
                    .Sort(sort)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving devices for user {UserId}", userId);
                return Enumerable.Empty<UserDevice>();
            }
        }

        public async Task<bool> RevokeDeviceAsync(int userId, string deviceId)
        {
            try
            {
                var filter = Builders<UserDevice>.Filter.And(
                    Builders<UserDevice>.Filter.Eq(d => d.UserId, userId),
                    Builders<UserDevice>.Filter.Eq(d => d.DeviceId, deviceId)
                );

                var result = await _devicesCollection.DeleteOneAsync(filter);
                
                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Device {DeviceId} revoked for user {UserId}", deviceId, userId);
                    return true;
                }

                _logger.LogWarning("Device {DeviceId} not found for user {UserId}", deviceId, userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking device {DeviceId} for user {UserId}", deviceId, userId);
                return false;
            }
        }

        /// <summary>
        /// Parses User-Agent string to extract browser and OS information
        /// </summary>
        private string ParseUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Device";

            try
            {
                var browser = "Unknown Browser";
                var os = "Unknown OS";

                // Detect browser
                if (userAgent.Contains("Edg/"))
                    browser = "Edge";
                else if (userAgent.Contains("Chrome/"))
                    browser = "Chrome";
                else if (userAgent.Contains("Firefox/"))
                    browser = "Firefox";
                else if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome"))
                    browser = "Safari";
                else if (userAgent.Contains("Opera/") || userAgent.Contains("OPR/"))
                    browser = "Opera";

                // Detect OS
                if (userAgent.Contains("Windows NT 10.0"))
                    os = "Windows 10";
                else if (userAgent.Contains("Windows NT 11.0"))
                    os = "Windows 11";
                else if (userAgent.Contains("Windows NT 6.3"))
                    os = "Windows 8.1";
                else if (userAgent.Contains("Windows NT 6.2"))
                    os = "Windows 8";
                else if (userAgent.Contains("Windows NT 6.1"))
                    os = "Windows 7";
                else if (userAgent.Contains("Mac OS X"))
                    os = "macOS";
                else if (userAgent.Contains("Linux"))
                    os = "Linux";
                else if (userAgent.Contains("Android"))
                    os = "Android";
                else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                    os = "iOS";

                return $"{browser} on {os}";
            }
            catch
            {
                return userAgent.Length > 50 ? userAgent.Substring(0, 50) + "..." : userAgent;
            }
        }
    }
}

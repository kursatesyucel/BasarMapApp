namespace BasarMapApp.Api.DTOs.Security
{
    public class UserDeviceDto
    {
        public string Id { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastLoginDate { get; set; }
        public DateTime FirstSeenDate { get; set; }
        public bool IsTrusted { get; set; }
        public bool IsCurrentDevice { get; set; }
    }
}

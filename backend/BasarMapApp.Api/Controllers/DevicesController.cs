using System.Security.Claims;
using BasarMapApp.Api.DTOs.Security;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all devices for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserDeviceDto>>>> GetMyDevices()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<IEnumerable<UserDeviceDto>>.FailureResult("Unable to identify user"));
                }

                var devices = await _deviceService.GetUserDevicesAsync(userId);
                
                // Get current device ID from header
                var currentDeviceId = Request.Headers["X-Device-Id"].ToString();

                var deviceDtos = devices.Select(d => new UserDeviceDto
                {
                    Id = d.Id ?? string.Empty,
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    LastLoginDate = d.LastLoginDate,
                    FirstSeenDate = d.FirstSeenDate,
                    IsTrusted = d.IsTrusted,
                    IsCurrentDevice = !string.IsNullOrEmpty(currentDeviceId) && d.DeviceId == currentDeviceId
                }).ToList();

                return Ok(ApiResponse<IEnumerable<UserDeviceDto>>.SuccessResult(deviceDtos, "Devices retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user devices");
                return StatusCode(500, ApiResponse<IEnumerable<UserDeviceDto>>.FailureResult("An error occurred while retrieving devices"));
            }
        }

        /// <summary>
        /// Revoke (delete) a device
        /// </summary>
        [HttpDelete("{deviceId}")]
        public async Task<ActionResult<ApiResponse<string>>> RevokeDevice(string deviceId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<string>.FailureResult("Unable to identify user"));
                }

                var success = await _deviceService.RevokeDeviceAsync(userId, deviceId);
                
                if (!success)
                {
                    return NotFound(ApiResponse<string>.FailureResult("Device not found"));
                }

                return Ok(ApiResponse<string>.SuccessResult("Device revoked successfully", "Device revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking device {DeviceId}", deviceId);
                return StatusCode(500, ApiResponse<string>.FailureResult("An error occurred while revoking device"));
            }
        }
    }
}

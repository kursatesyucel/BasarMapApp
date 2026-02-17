using BasarMapApp.Api.DTOs.Auth;
using BasarMapApp.Api.Filters;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Responses;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasarMapApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public AuthController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] UserRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResult("Invalid input data"));
            }

            var (success, message) = await _authService.RegisterAsync(registerDto, HttpContext);
            if (!success)
            {
                return Conflict(ApiResponse<string>.FailureResult(message ?? "Registration failed"));
            }

            return Ok(ApiResponse<string>.SuccessResult(message ?? "Registration successful", message ?? "Registration successful"));
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<ApiResponse<string>>> VerifyEmail([FromBody] VerifyCodeDto verifyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResult("Invalid input data"));
            }

            var (success, message) = await _authService.VerifyEmailAsync(verifyDto);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.FailureResult(message ?? "Verification failed"));
            }

            return Ok(ApiResponse<string>.SuccessResult(message ?? "Verification successful", message ?? "Verification successful"));
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDto forgotDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResult("Invalid input data"));
            }

            var (success, message) = await _authService.ForgotPasswordAsync(forgotDto, HttpContext);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.FailureResult(message));
            }

            return Ok(ApiResponse<string>.SuccessResult(message, message));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordDto resetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResult("Invalid input data"));
            }

            var (success, message) = await _authService.ResetPasswordAsync(resetDto);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.FailureResult(message));
            }

            return Ok(ApiResponse<string>.SuccessResult(message, message));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResult("Invalid input data"));
            }

            var result = await _authService.LoginAsync(loginDto, HttpContext);
            if (result == null)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.FailureResult("Invalid credentials, email not verified, or account is inactive"));
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.FailureResult(result.ErrorMessage));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Login successful"));
        }

        /// <summary>
        /// Get all users - Admin only
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserListDto>>>> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<UserListDto>>.SuccessResult(users, "Users retrieved successfully"));
        }

        /// <summary>
        /// Update user role - Admin only
        /// </summary>
        [HttpPut("users/{id}/role")]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "User", action: "UpdateRole")]
        public async Task<ActionResult<ApiResponse<UserListDto>>> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<UserListDto>.FailureResult("Invalid input data"));
            }

            // Validate role
            var validRoles = new[] { "User", "Manager", "Admin" };
            if (!validRoles.Contains(updateDto.Role))
            {
                return BadRequest(ApiResponse<UserListDto>.FailureResult("Invalid role. Valid roles are: User, Manager, Admin"));
            }

            var user = await _userRepository.UpdateRoleAsync(id, updateDto.Role);
            if (user == null)
            {
                return NotFound(ApiResponse<UserListDto>.FailureResult("User not found"));
            }

            var userDto = new UserListDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsEmailConfirmed = user.IsEmailConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResponse<UserListDto>.SuccessResult(userDto, "User role updated successfully"));
        }

        /// <summary>
        /// Update user status (active/inactive) - Admin only
        /// </summary>
        [HttpPut("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "User", action: "UpdateStatus")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.FailureResult("Invalid input data"));
            }

            // Get current admin user ID from claims
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
            {
                return Unauthorized(ApiResponse<string>.FailureResult("Unable to identify admin user"));
            }

            var (success, message) = await _authService.UpdateUserStatusAsync(id, updateDto.IsActive, adminId);
            if (!success)
            {
                if (message?.Contains("cannot change your own") == true)
                    return BadRequest(ApiResponse<string>.FailureResult(message));
                
                return NotFound(ApiResponse<string>.FailureResult(message ?? "Failed to update user status"));
            }

            return Ok(ApiResponse<string>.SuccessResult(message ?? "User status updated", message ?? "User status updated"));
        }

        /// <summary>
        /// Delete user - Admin only (WARNING: This permanently deletes the user from database)
        /// </summary>
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        [AuditLog(entityName: "User", action: "Delete")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
        {
            // Get current admin user ID from claims
            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out int adminId))
            {
                return Unauthorized(ApiResponse<string>.FailureResult("Unable to identify admin user"));
            }

            var (success, message) = await _authService.DeleteUserAsync(id, adminId);
            if (!success)
            {
                if (message?.Contains("cannot delete your own") == true)
                    return BadRequest(ApiResponse<string>.FailureResult(message));
                
                return NotFound(ApiResponse<string>.FailureResult(message ?? "Failed to delete user"));
            }

            return Ok(ApiResponse<string>.SuccessResult(message ?? "User deleted", message ?? "User deleted"));
        }
    }
}

using BasarMapApp.Api.DTOs.Auth;
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
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] UserRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResult("Invalid input data"));
            }

            var result = await _authService.RegisterAsync(registerDto);
            if (result == null)
            {
                return Conflict(ApiResponse<AuthResponseDto>.FailureResult("Username already exists"));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "User registered successfully"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResult("Invalid input data"));
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.FailureResult("Invalid username or password"));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResult(result, "Login successful"));
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserListDto>>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            });

            return Ok(ApiResponse<IEnumerable<UserListDto>>.SuccessResult(userDtos, "Users retrieved successfully"));
        }

        [HttpPut("users/{id}/role")]
        [Authorize(Roles = "Admin")]
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
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(ApiResponse<UserListDto>.SuccessResult(userDto, "User role updated successfully"));
        }
    }
}

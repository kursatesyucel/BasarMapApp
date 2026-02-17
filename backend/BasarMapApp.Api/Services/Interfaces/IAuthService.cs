using BasarMapApp.Api.DTOs.Auth;
using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string? Message)> RegisterAsync(UserRegisterDto registerDto, HttpContext? httpContext = null);
        Task<(bool Success, string? Message)> VerifyEmailAsync(VerifyCodeDto verifyDto);
        Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto, HttpContext? httpContext = null);
        string GenerateToken(User user);
        
        // Admin user management methods
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<(bool Success, string? Message)> UpdateUserStatusAsync(int userId, bool isActive, int adminId);
        Task<(bool Success, string? Message)> DeleteUserAsync(int userId, int adminId);
    }
}

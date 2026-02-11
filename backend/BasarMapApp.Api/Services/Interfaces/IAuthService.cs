using BasarMapApp.Api.DTOs.Auth;
using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string? Message)> RegisterAsync(UserRegisterDto registerDto);
        Task<(bool Success, string? Message)> VerifyEmailAsync(VerifyCodeDto verifyDto);
        Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto);
        string GenerateToken(User user);
    }
}

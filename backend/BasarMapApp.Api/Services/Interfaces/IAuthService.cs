using BasarMapApp.Api.DTOs.Auth;
using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(UserRegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto);
        string GenerateToken(User user);
    }
}

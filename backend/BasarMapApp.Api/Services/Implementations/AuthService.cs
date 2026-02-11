using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BasarMapApp.Api.DTOs.Auth;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Repositories.Interfaces;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BasarMapApp.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(UserRegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUser != null)
                return null;

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create user
            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            // Generate token
            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto)
        {
            // Find user
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            // Generate token
            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        public string GenerateToken(User user)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT Secret is not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

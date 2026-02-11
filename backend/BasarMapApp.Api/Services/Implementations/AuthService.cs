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
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository, 
            IMailService mailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _mailService = mailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool Success, string? Message)> RegisterAsync(UserRegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                var existingUsername = await _userRepository.GetByUsernameAsync(registerDto.Username);
                if (existingUsername != null)
                    return (false, "Username already exists");

                // Check if email already exists
                var existingEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingEmail != null)
                    return (false, "Email already exists");

                // Generate 6-digit verification code
                var verificationCode = GenerateVerificationCode();

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                // Create user
                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    Role = "User",
                    VerificationCode = verificationCode,
                    IsEmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user);

                // Send verification email
                var emailSent = await _mailService.SendVerificationEmailAsync(
                    user.Email, 
                    user.Username, 
                    verificationCode);

                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send verification email to {Email}", user.Email);
                    return (false, "User registered but failed to send verification email. Please contact support.");
                }

                return (true, "Registration successful! Please check your email for verification code.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return (false, "An error occurred during registration");
            }
        }

        public async Task<(bool Success, string? Message)> VerifyEmailAsync(VerifyCodeDto verifyDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(verifyDto.Email);
                if (user == null)
                    return (false, "User not found");

                if (user.IsEmailConfirmed)
                    return (false, "Email already verified");

                if (string.IsNullOrEmpty(user.VerificationCode))
                    return (false, "No verification code found");

                if (user.VerificationCode != verifyDto.Code)
                    return (false, "Invalid verification code");

                // Verify email
                user.IsEmailConfirmed = true;
                user.VerificationCode = null; // Clear the code after verification
                await _userRepository.UpdateAsync(user);

                // Send welcome email
                await _mailService.SendWelcomeEmailAsync(user.Email, user.Username);

                return (true, "Email verified successfully! You can now login.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return (false, "An error occurred during verification");
            }
        }

        public async Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto)
        {
            try
            {
                // Find user by username or email
                var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.LoginIdentifier);
                if (user == null)
                    return null;

                // Check if email is verified
                if (!user.IsEmailConfirmed)
                {
                    _logger.LogWarning("Login attempt with unverified email: {Email}", user.Email);
                    return null;
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return null;
            }
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
                new Claim(ClaimTypes.Email, user.Email),
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

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}

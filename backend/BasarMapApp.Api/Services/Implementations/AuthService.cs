using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BasarMapApp.Api.DTOs.Auth;
using BasarMapApp.Api.Models;
using BasarMapApp.Api.Models.Logs;
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
        private readonly ILogService _logService;

        public AuthService(
            IUserRepository userRepository, 
            IMailService mailService,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            ILogService logService)
        {
            _userRepository = userRepository;
            _mailService = mailService;
            _configuration = configuration;
            _logger = logger;
            _logService = logService;
        }

        public async Task<(bool Success, string? Message)> RegisterAsync(UserRegisterDto registerDto, HttpContext? httpContext = null)
        {
            // Extract HTTP context information for logging
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

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

                // Log successful user registration (Audit Log)
                await LogUserRegistrationAsync(user.Id, user.Email, user.Username, ipAddress);

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

        /// <summary>
        /// Helper method to log user registration asynchronously without blocking
        /// </summary>
        private async Task LogUserRegistrationAsync(int userId, string email, string username, string? ipAddress)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "Register",
                    EntityName = "User",
                    EntityId = userId.ToString(),
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    NewValues = new
                    {
                        Email = email,
                        Username = username,
                        Role = "User",
                        IsEmailConfirmed = false
                    }
                };

                // Fire-and-forget: Logging should not block registration process
                await _logService.CreateAuditLogAsync(auditLog);
            }
            catch (Exception ex)
            {
                // Swallow exceptions - logging failures should never break registration
                _logger.LogError(ex, "Failed to create registration audit log. Logging error is ignored.");
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

        public async Task<AuthResponseDto?> LoginAsync(UserLoginDto loginDto, HttpContext? httpContext = null)
        {
            // Extract HTTP context information for logging
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            string? userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            try
            {
                // Find user by username or email
                var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.LoginIdentifier);
                if (user == null)
                {
                    // Log failed login attempt - user not found
                    await LogLoginAttemptAsync(
                        userId: null,
                        email: loginDto.LoginIdentifier,
                        isSuccess: false,
                        failureReason: "User not found",
                        ipAddress: ipAddress,
                        userAgent: userAgent
                    );
                    return null;
                }

                // ========== ADIM 1: Kilit Kontrolü (Brute-Force - En Başta) ==========
                if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                {
                    var lockoutEndLocal = TimeZoneInfo.ConvertTimeFromUtc(user.LockoutEnd.Value, GetTurkeyTimeZone());
                    var lockoutMessage = $"Hesabınız çok fazla hatalı giriş denemesi nedeniyle {lockoutEndLocal:dd.MM.yyyy HH:mm} tarihine kadar kilitlenmiştir.";
                    _logger.LogWarning("Locked account login attempt: {Email}", user.Email);

                    await LogLoginAttemptAsync(
                        userId: user.Id,
                        email: user.Email,
                        isSuccess: false,
                        failureReason: "Account locked",
                        ipAddress: ipAddress,
                        userAgent: userAgent
                    );
                    return new AuthResponseDto { Token = string.Empty, Username = string.Empty, Role = string.Empty, ErrorMessage = lockoutMessage };
                }

                // Lockout süresi dolmuşsa sıfırla
                if (user.LockoutEnd.HasValue && user.LockoutEnd.Value <= DateTime.UtcNow)
                {
                    user.LockoutEnd = null;
                    user.AccessFailedCount = 0;
                    await _userRepository.UpdateAsync(user);
                }

                // Check if email is verified
                if (!user.IsEmailConfirmed)
                {
                    _logger.LogWarning("Login attempt with unverified email: {Email}", user.Email);
                    
                    await LogLoginAttemptAsync(
                        userId: user.Id,
                        email: user.Email,
                        isSuccess: false,
                        failureReason: "Email not verified",
                        ipAddress: ipAddress,
                        userAgent: userAgent
                    );
                    return null;
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt with inactive account: {Username}", user.Username);
                    
                    await LogLoginAttemptAsync(
                        userId: user.Id,
                        email: user.Email,
                        isSuccess: false,
                        failureReason: "Account is inactive",
                        ipAddress: ipAddress,
                        userAgent: userAgent
                    );
                    return null;
                }

                // ========== ADIM 2 & 3: Şifre Doğrulama ==========
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    // Hatalı şifre - AccessFailedCount artır
                    user.AccessFailedCount++;

                    if (user.AccessFailedCount >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddHours(1);
                        user.AccessFailedCount = 5;
                        await _userRepository.UpdateAsync(user);

                        await LogLoginAttemptAsync(
                            userId: user.Id,
                            email: user.Email,
                            isSuccess: false,
                            failureReason: "Account locked after 5 failed attempts",
                            ipAddress: ipAddress,
                            userAgent: userAgent
                        );
                        return new AuthResponseDto { Token = string.Empty, Username = string.Empty, Role = string.Empty, ErrorMessage = "Hesabınız 1 saatliğine kilitlendi." };
                    }

                    await _userRepository.UpdateAsync(user);
                    var remainingAttempts = 5 - user.AccessFailedCount;

                    await LogLoginAttemptAsync(
                        userId: user.Id,
                        email: user.Email,
                        isSuccess: false,
                        failureReason: "Invalid password",
                        ipAddress: ipAddress,
                        userAgent: userAgent
                    );
                    return new AuthResponseDto { Token = string.Empty, Username = string.Empty, Role = string.Empty, ErrorMessage = $"Hatalı şifre. Kalan hakkınız: {remainingAttempts}" };
                }

                // ========== Başarılı Giriş - Sayaçları Sıfırla ==========
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                await _userRepository.UpdateAsync(user);

                // Login successful - generate token
                var token = GenerateToken(user);

                await LogLoginAttemptAsync(
                    userId: user.Id,
                    email: user.Email,
                    isSuccess: true,
                    failureReason: null,
                    ipAddress: ipAddress,
                    userAgent: userAgent
                );

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
                
                await LogLoginAttemptAsync(
                    userId: null,
                    email: loginDto.LoginIdentifier,
                    isSuccess: false,
                    failureReason: $"System error: {ex.Message}",
                    ipAddress: ipAddress,
                    userAgent: userAgent
                );
                
                return null;
            }
        }

        /// <summary>
        /// Helper method to log login attempts asynchronously without blocking
        /// </summary>
        private async Task LogLoginAttemptAsync(
            int? userId,
            string email,
            bool isSuccess,
            string? failureReason,
            string? ipAddress,
            string? userAgent)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    UserId = userId,
                    Email = email,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = isSuccess,
                    FailureReason = failureReason
                };

                // Fire-and-forget: Logging should not block login process
                await _logService.CreateLoginLogAsync(loginLog);
            }
            catch (Exception ex)
            {
                // Swallow exceptions - logging failures should never break authentication
                _logger.LogError(ex, "Failed to create login log. Logging error is ignored.");
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

        // Admin user management methods
        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsEmailConfirmed = u.IsEmailConfirmed,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<(bool Success, string? Message)> UpdateUserStatusAsync(int userId, bool isActive, int adminId)
        {
            try
            {
                // Prevent admin from deactivating themselves
                if (userId == adminId)
                    return (false, "You cannot change your own account status");

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return (false, "User not found");

                var updatedUser = await _userRepository.UpdateStatusAsync(userId, isActive);
                if (updatedUser == null)
                    return (false, "Failed to update user status");

                var status = isActive ? "activated" : "deactivated";
                _logger.LogInformation("User {UserId} was {Status} by admin {AdminId}", userId, status, adminId);

                return (true, $"User successfully {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status");
                return (false, "An error occurred while updating user status");
            }
        }

        public async Task<(bool Success, string? Message)> DeleteUserAsync(int userId, int adminId)
        {
            try
            {
                // Prevent admin from deleting themselves
                if (userId == adminId)
                    return (false, "You cannot delete your own account");

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return (false, "User not found");

                var deleted = await _userRepository.DeleteAsync(userId);
                if (!deleted)
                    return (false, "Failed to delete user");

                _logger.LogInformation("User {UserId} was deleted by admin {AdminId}", userId, adminId);

                return (true, "User successfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return (false, "An error occurred while deleting user");
            }
        }

        private static TimeZoneInfo GetTurkeyTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            }
        }
    }
}

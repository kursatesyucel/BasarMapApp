using System.ComponentModel.DataAnnotations;

namespace BasarMapApp.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";
        
        [MaxLength(10)]
        public string? VerificationCode { get; set; }
        
        public bool IsEmailConfirmed { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Number of failed login attempts (Brute-Force protection)
        /// </summary>
        public int AccessFailedCount { get; set; } = 0;

        /// <summary>
        /// When the lockout ends (UTC). Null if not locked.
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Token for password reset flow.
        /// </summary>
        public string? PasswordResetToken { get; set; }

        /// <summary>
        /// When the password reset token expires (UTC).
        /// </summary>
        public DateTime? PasswordResetTokenExpires { get; set; }
    }
}

using System.Net;
using System.Net.Mail;
using BasarMapApp.Api.Services.Interfaces;

namespace BasarMapApp.Api.Services.Implementations
{
    public class GmailMailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailMailService> _logger;

        public GmailMailService(IConfiguration configuration, ILogger<GmailMailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string username, string verificationCode)
        {
            try
            {
                var subject = "BasarMapApp - Email Verification";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color: #667eea; text-align: center;'>Welcome to BasarMapApp!</h2>
                            <p>Hello <strong>{username}</strong>,</p>
                            <p>Thank you for registering with BasarMapApp. Please verify your email address to complete your registration.</p>
                            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                                <p style='margin: 0; font-size: 14px; color: #666;'>Your verification code is:</p>
                                <h1 style='color: #667eea; letter-spacing: 5px; margin: 10px 0; font-size: 36px;'>{verificationCode}</h1>
                            </div>
                            <p style='color: #666; font-size: 14px;'>This code will expire in 24 hours.</p>
                            <p style='color: #666; font-size: 14px;'>If you didn't create an account with BasarMapApp, please ignore this email.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            <p style='text-align: center; color: #999; font-size: 12px;'>
                                © 2026 BasarMapApp. All rights reserved.
                            </p>
                        </div>
                    </body>
                    </html>
                ";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string username)
        {
            try
            {
                var subject = "Welcome to BasarMapApp!";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color: #667eea; text-align: center;'>🎉 Welcome to BasarMapApp!</h2>
                            <p>Hello <strong>{username}</strong>,</p>
                            <p>Your email has been successfully verified! You can now access all features of BasarMapApp.</p>
                            <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #4caf50;'>
                                <p style='margin: 0; color: #2e7d32;'><strong>✓ Email Verified</strong></p>
                                <p style='margin: 5px 0 0 0; color: #555;'>You can now login and start using the application.</p>
                            </div>
                            <p>Thank you for joining us!</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                            <p style='text-align: center; color: #999; font-size: 12px;'>
                                © 2026 BasarMapApp. All rights reserved.
                            </p>
                        </div>
                    </body>
                    </html>
                ";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var mailSettings = _configuration.GetSection("MailSettings");
                var fromEmail = mailSettings["Email"];
                var password = mailSettings["Password"];
                var host = mailSettings["Host"] ?? "smtp.gmail.com";
                var port = int.Parse(mailSettings["Port"] ?? "587");

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("Mail settings are not configured properly");
                    return false;
                }

                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromEmail, password),
                    Timeout = 10000 // 10 seconds
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "BasarMapApp"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {Email}. Status: {Status}", toEmail, ex.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {Email}", toEmail);
                return false;
            }
        }
    }
}

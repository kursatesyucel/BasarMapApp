namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail, string username, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string username);
        Task<bool> SendPasswordResetLinkAsync(string toUserEmail, string resetToken);
        Task<bool> SendNewDeviceAlertAsync(string toEmail, string username, string deviceName, string ipAddress);
    }
}

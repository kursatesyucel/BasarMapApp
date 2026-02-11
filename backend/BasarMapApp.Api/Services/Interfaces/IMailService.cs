namespace BasarMapApp.Api.Services.Interfaces
{
    public interface IMailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail, string username, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string username);
    }
}

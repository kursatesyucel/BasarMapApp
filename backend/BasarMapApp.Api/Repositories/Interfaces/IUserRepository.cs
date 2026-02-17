using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task<User?> GetByUsernameOrEmailAsync(string identifier);
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> UpdateRoleAsync(int id, string role);
        Task<User?> UpdateStatusAsync(int id, bool isActive);
        Task<bool> DeleteAsync(int id);
    }
}

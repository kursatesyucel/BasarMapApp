using BasarMapApp.Api.Models;

namespace BasarMapApp.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> UpdateRoleAsync(int id, string role);
        Task<User?> GetByIdAsync(int id);
    }
}

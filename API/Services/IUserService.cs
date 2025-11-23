using Model;

namespace API.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task CreateAsync(User user);
        Task UpdateAsync(string id, User user);
        Task DeleteAsync(string id);
        Task UpdateUserByClientIdAsync(string clientId, User updatedUser);
        Task<User?> GetUserByClientIdAsync(string clientId);
        Task UpdateUserAsync(string? id, User updatedUser);
    }
}

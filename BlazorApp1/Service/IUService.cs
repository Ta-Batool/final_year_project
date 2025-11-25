using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface IUService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(string id, User user);
        Task DeleteUserAsync(string id);

        Task<User?> GetUserByClientIdAsync(string clientId);
        Task UpdateUserByClientIdAsync(string clientId, User user);
    }
}

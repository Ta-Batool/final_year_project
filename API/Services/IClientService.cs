using Model;

namespace API.Services
{
    public interface IClientService
    {
        Task<List<Client>> GetAllAsync();
        Task<Client?> GetByEmailAsync(string email);
        Task CreateAsync(Client client);
        Task UpdateAsync(string email, Client client);
        Task DeleteAsync(string email);
    }
}

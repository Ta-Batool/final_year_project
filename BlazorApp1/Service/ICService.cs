using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface ICService
    {
        Task<Client?> GetClientByEmailAsync(string email);
        Task<List<Client>> GetAllClientsAsync();
        Task AddClientAsync(Client client);
        Task UpdateClientByEmailAsync(string email, Client client);
        Task DeleteClientAsync(string email);
    }
}

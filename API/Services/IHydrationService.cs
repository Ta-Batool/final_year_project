using Model;

namespace API.Services
{
    public interface IHydrationService
    {
        Task<List<HydrationLog>> GetForDayAsync(string clientId, DateTime dateUtc);
        Task<HydrationLog> AddAsync(HydrationLog log);
        Task DeleteAsync(string id);
    }
}

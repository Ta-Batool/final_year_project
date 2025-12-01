using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IHydrationService
    {
        Task<HydrationLog?> GetTodayAsync(string clientId);
        Task AddWaterAsync(string clientId, int amountMl);
        Task UpdateTargetAsync(string clientId, int targetMl);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace API.Services
{
    public interface IHydrationService
    {
        // Get hydration logs for a given client + day (UTC date)
        Task<List<HydrationLog>> GetForDayAsync(string clientId, DateTime dateUtc);

        // Insert a hydration log document
        Task<HydrationLog> AddAsync(HydrationLog log);

        // Increment today's water for a client
        Task AddWaterAsync(string clientId, int amountMl);

        // Set / update today's target for a client
        Task UpdateTargetAsync(string clientId, int targetMl);

        // Delete by id
        Task DeleteAsync(string id);
    }
}

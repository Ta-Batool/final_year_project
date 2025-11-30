using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IHydrationService
    {
        Task<List<HydrationLog>> GetForDateAsync(string clientId, DateTime date);
        Task AddAsync(HydrationLog log);
        Task DeleteAsync(string id);
    }
}

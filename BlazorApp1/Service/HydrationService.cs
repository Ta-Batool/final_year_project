using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class HydrationService : IHydrationService
    {
        private readonly HttpClient _http;

        public HydrationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<HydrationLog>> GetForDateAsync(string clientId, DateTime date)
        {
            var dateParam = date.ToString("yyyy-MM-dd");
            var result = await _http.GetFromJsonAsync<List<HydrationLog>>(
                $"api/hydration/by-date/{clientId}?date={dateParam}");

            return result ?? new List<HydrationLog>();
        }

        public async Task AddAsync(HydrationLog log)
        {
            var response = await _http.PostAsJsonAsync("api/hydration", log);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/hydration/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}

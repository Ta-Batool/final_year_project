using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<HydrationLog?> GetTodayAsync(string clientId)
        {
            try
            {
                // 🔹 API is returning a LIST, so read as List<HydrationLog>
                var list = await _http.GetFromJsonAsync<List<HydrationLog>>(
                    $"api/hydration/today/{clientId}");

                return list?.FirstOrDefault();
            }
            catch (HttpRequestException)
            {
                // e.g. 404 → no hydration yet
                return null;
            }
        }

        public async Task AddWaterAsync(string clientId, int amountMl)
        {
            var payload = new { ClientId = clientId, AmountMl = amountMl };
            var response = await _http.PostAsJsonAsync("api/hydration/add", payload);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateTargetAsync(string clientId, int targetMl)
        {
            var payload = new { ClientId = clientId, TargetMl = targetMl };
            var response = await _http.PostAsJsonAsync("api/hydration/target", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public class MetabolismApiService
    {
        private readonly HttpClient _http;

        public MetabolismApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<dynamic?> GetSummaryAsync(string userId)
        {
            return await _http.GetFromJsonAsync<dynamic>(
                $"api/metabolism/summary/{userId}");
        }

        public async Task<List<dynamic>> GetTimelineAsync(string userId, int days = 30)
        {
            return await _http.GetFromJsonAsync<List<dynamic>>(
                $"api/metabolism/timeline/{userId}?days={days}")
                   ?? new List<dynamic>();
        }
    }
}

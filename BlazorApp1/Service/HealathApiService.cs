using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class HealthApiService
    {
        private readonly HttpClient _http;

        public HealthApiService(HttpClient http)
        {
            _http = http;
        }

        // ---------- BP ----------
        public async Task<List<BPLog>?> GetBPAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<BPLog>>($"api/health/bp/{userId}");
        }

        public async Task AddBPAsync(BPLog log)
        {
            var res = await _http.PostAsJsonAsync("api/health/bp", log);
            res.EnsureSuccessStatusCode();
        }

        // ---------- GLUCOSE ----------
        public async Task<List<GlucoseLog>?> GetGlucoseAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<GlucoseLog>>($"api/health/glucose/{userId}");
        }

        public async Task AddGlucoseAsync(GlucoseLog log)
        {
            var res = await _http.PostAsJsonAsync("api/health/glucose", log);
            res.EnsureSuccessStatusCode();
        }

        // ---------- WEIGHT ----------
        public async Task<List<WeightLog>?> GetWeightAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<WeightLog>>($"api/health/weight/{userId}");
        }

        public async Task AddWeightAsync(WeightLog log)
        {
            var res = await _http.PostAsJsonAsync("api/health/weight", log);
            res.EnsureSuccessStatusCode();
        }
    }
}

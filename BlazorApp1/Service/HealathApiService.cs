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
        public async Task<List<BPLog>> GetBPAsync(string userId)
            => await GetOrThrow<List<BPLog>>($"api/healthlogs/bp/{userId}") ?? new();

        public async Task AddBPAsync(BPLog log)
            => await PostOrThrow("api/healthlogs/bp", log);

        // ---------- GLUCOSE ----------
        public async Task<List<GlucoseLog>> GetGlucoseAsync(string userId)
            => await GetOrThrow<List<GlucoseLog>>($"api/healthlogs/glucose/{userId}") ?? new();

        public async Task AddGlucoseAsync(GlucoseLog log)
            => await PostOrThrow("api/healthlogs/glucose", log);

        // ---------- WEIGHT ----------
        public async Task<List<WeightLog>> GetWeightAsync(string userId)
            => await GetOrThrow<List<WeightLog>>($"api/healthlogs/weight/{userId}") ?? new();

        public async Task AddWeightAsync(WeightLog log)
            => await PostOrThrow("api/healthlogs/weight", log);

        // ---------- Helpers ----------
        private async Task<T?> GetOrThrow<T>(string url)
        {
            using var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                throw new Exception($"GET {url} failed: {(int)res.StatusCode} {res.ReasonPhrase}. {msg}");
            }
            return await res.Content.ReadFromJsonAsync<T>();
        }

        private async Task PostOrThrow<TBody>(string url, TBody body)
        {
            using var res = await _http.PostAsJsonAsync(url, body);
            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                throw new Exception($"POST {url} failed: {(int)res.StatusCode} {res.ReasonPhrase}. {msg}");
            }
        }
    }
}

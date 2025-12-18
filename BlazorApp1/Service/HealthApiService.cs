using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class HealthApiService
    {
        private readonly HttpClient _http;
        public HealthApiService(HttpClient http) => _http = http;

        public Task<List<BPLog>?> GetBpAsync(string userId) =>
            _http.GetFromJsonAsync<List<BPLog>>($"api/health/bp/{userId}");

        public Task<List<GlucoseLog>?> GetGlucoseAsync(string userId) =>
            _http.GetFromJsonAsync<List<GlucoseLog>>($"api/health/glucose/{userId}");

        public Task<List<WeightLog>?> GetWeightAsync(string userId) =>
            _http.GetFromJsonAsync<List<WeightLog>>($"api/health/weight/{userId}");

        public async Task<(BPLog saved, object? alert)> AddBpAsync(BPLog log)
        {
            var res = await _http.PostAsJsonAsync("api/health/bp", log);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return (log, json?.ContainsKey("alert") == true ? json["alert"] : null);
        }
    }
}

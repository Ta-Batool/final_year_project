using System.Net.Http.Json;

namespace BlazorApp1.Service
{
    public class WeightLogApiService
    {
        private readonly HttpClient _http;

        public WeightLogApiService(HttpClient http)
        {
            _http = http;
        }

        // GET api/healthlogs/weight/{userId}
        public async Task<List<WeightLogDto>?> GetByUserAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<WeightLogDto>>($"api/healthlogs/weight/{userId}");
        }

        // POST api/healthlogs/weight
        public async Task<WeightLogDto?> CreateAsync(CreateWeightLogRequest req)
        {
            var res = await _http.PostAsJsonAsync("api/healthlogs/weight", req);
            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                throw new Exception($"{(int)res.StatusCode}: {msg}");
            }

            return await res.Content.ReadFromJsonAsync<WeightLogDto>();
        }
    }

    // ---------- DTOs ----------
    public record CreateWeightLogRequest(
        string UserId,
        double WeightKg,
        double? BodyFatPercent,
        string? Notes
    );

    public class WeightLogDto
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public double WeightKg { get; set; }
        public double? BodyFatPercent { get; set; }
        public string? Notes { get; set; }
        public DateTime LoggedAt { get; set; }
    }
}

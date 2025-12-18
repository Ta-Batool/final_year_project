using System.Net.Http.Json;

namespace BlazorApp1.Service
{
    public class GlucoseLogApiService
    {
        private readonly HttpClient _http;

        public GlucoseLogApiService(HttpClient http)
        {
            _http = http;
        }

        // GET api/healthlogs/glucose/{userId}
        public async Task<List<GlucoseLogDto>?> GetByUserAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<GlucoseLogDto>>($"api/healthlogs/glucose/{userId}");
        }

        // POST api/healthlogs/glucose
        public async Task<GlucoseLogDto?> CreateAsync(CreateGlucoseLogRequest req)
        {
            var res = await _http.PostAsJsonAsync("api/healthlogs/glucose", req);
            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                throw new Exception($"{(int)res.StatusCode}: {msg}");
            }

            return await res.Content.ReadFromJsonAsync<GlucoseLogDto>();
        }
    }

    // ---------- DTOs ----------
    public record CreateGlucoseLogRequest(
        string UserId,
        int Value,
        string Type,     // "Fasting" | "Random" | "PostMeal"
        string? Notes
    );

    public class GlucoseLogDto
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public int Value { get; set; }
        public string? Type { get; set; }
        public string? Notes { get; set; }
        public DateTime LoggedAt { get; set; }
    }
}

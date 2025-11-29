using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public CalorieNinjaService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiNinjas:ApiKey"] ?? "";

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                Console.WriteLine("⚠️ ApiNinjas:ApiKey is missing from configuration.");
            }
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("API Ninjas key not configured.");

            var url = $"https://api.api-ninjas.com/v1/nutrition?query={Uri.EscapeDataString(query)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-Api-Key", _apiKey);

            using var res = await _http.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ ApiNinjas error {res.StatusCode}: {json}");
                return new List<NutritionItemDto>(); // keep UI silent but log on server
            }

            return JsonSerializer.Deserialize<List<NutritionItemDto>>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new List<NutritionItemDto>();
        }
    }
}

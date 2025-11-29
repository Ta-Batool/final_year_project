using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public class ApiNinjasService : ICalorieNinjaService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public ApiNinjasService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiNinjas:ApiKey"] ?? "";
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("API Ninjas key not configured.");

            var url = $"https://api.api-ninjas.com/v1/nutrition?query={Uri.EscapeDataString(query)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-Api-Key", _apiKey);

            var res = await _http.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine("API Ninja ERROR → " + json);
                return new List<NutritionItemDto>();
            }

            return JsonSerializer.Deserialize<List<NutritionItemDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<NutritionItemDto>();
        }
    }
}

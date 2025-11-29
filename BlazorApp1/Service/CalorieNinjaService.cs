using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
{
    private readonly HttpClient _http;
    private readonly ILogger<CalorieNinjaService> _logger;
    private const string BaseUrl = "https://api.api-ninjas.com/v1/nutrition?query=";
    // or whatever URL you’re using

    public CalorieNinjaService(HttpClient http, ILogger<CalorieNinjaService> logger,
                               IConfiguration config)
    {
        _http = http;
        _logger = logger;

        var apiKey = config["CalorieNinjas:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }
    }

    public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
    {
        var url = $"{BaseUrl}{Uri.EscapeDataString(query)}";
        using var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Nutrition API failed: {Status} {Reason}", 
                               (int)response.StatusCode, response.ReasonPhrase);
            return new List<NutritionItemDto>();
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            var items = System.Text.Json.JsonSerializer
                .Deserialize<List<NutritionItemDto>>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return items ?? new List<NutritionItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Food suggestions failed. Raw JSON: {Json}", json);
            return new List<NutritionItemDto>();
        }
    }
}

}

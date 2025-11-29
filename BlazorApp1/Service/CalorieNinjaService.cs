using System.Text.Json;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CalorieNinjaService> _logger;

        public CalorieNinjaService(HttpClient httpClient, ILogger<CalorieNinjaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<NutritionItemDto>();

            try
            {
                // API Ninjas endpoint
                var url = $"v1/nutrition?query={Uri.EscapeDataString(query)}";

                using var response = await _httpClient.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Nutrition API failed. Status: {Status}, Body: {Body}",
                        (int)response.StatusCode,
                        body
                    );
                    return new List<NutritionItemDto>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var rawItems = JsonSerializer.Deserialize<List<NutritionApiItem>>(body, options)
                              ?? new List<NutritionApiItem>();

                return rawItems.Select(x => new NutritionItemDto
                {
                    Name = x.Name ?? string.Empty,
                    Calories = x.GetCaloriesAsDouble()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Nutrition API");
                return new List<NutritionItemDto>();
            }
        }

        // Matches API Ninjas JSON shape and safely parses "calories"
        private class NutritionApiItem
        {
            public string? Name { get; set; }

            // "calories" can be a number OR a string like "Only available for premium subscribers."
            public JsonElement Calories { get; set; }

            public double? GetCaloriesAsDouble()
            {
                if (Calories.ValueKind == JsonValueKind.Number &&
                    Calories.TryGetDouble(out var d))
                {
                    return d;
                }

                if (Calories.ValueKind == JsonValueKind.String &&
                    double.TryParse(Calories.GetString(), out var parsed))
                {
                    return parsed;
                }

                return null; // free tier might not return calories at all
            }
        }
    }
}

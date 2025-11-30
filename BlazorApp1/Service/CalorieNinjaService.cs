using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CalorieNinjaService> _logger;
        private readonly string _apiKey;
        private readonly JsonSerializerOptions _jsonOptions;

        public CalorieNinjaService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CalorieNinjaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey = configuration["USDA:ApiKey"]
                      ?? throw new InvalidOperationException("USDA:ApiKey is not configured.");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<NutritionItemDto>?> GetNutritionAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<NutritionItemDto>();

            var url = $"foods/search?query={Uri.EscapeDataString(query)}&pageSize=10&api_key={_apiKey}";

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("USDA API failed. Status: {StatusCode}, Body: {Body}",
                        response.StatusCode, body);
                    return new List<NutritionItemDto>();
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                var data = JsonSerializer.Deserialize<FoodSearchResponse>(json, _jsonOptions);

                if (data?.Foods == null || data.Foods.Count == 0)
                    return new List<NutritionItemDto>();

                var result = data.Foods
                    .Select(f =>
                    {
                        // pick nutrient 208 = Energy (kcal)
                        var energy = f.FoodNutrients?
                            .FirstOrDefault(n =>
                                string.Equals(n.NutrientNumber, "208", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(n.NutrientName, "Energy", StringComparison.OrdinalIgnoreCase));

                        return new NutritionItemDto
                        {
                            Name = f.Description ?? string.Empty,
                            Calories = energy?.Value
                        };
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .ToList();

                return result;
            }
            catch (OperationCanceledException)
            {
                // expected when user is typing fast / debounce cancels
                return new List<NutritionItemDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling USDA FoodData Central API.");
                return new List<NutritionItemDto>();
            }
        }

        // Internal DTOs just for deserialization of USDA response
        private class FoodSearchResponse
        {
            public List<FoodItem> Foods { get; set; } = new();
        }

        private class FoodItem
        {
            public string? Description { get; set; }
            public List<FoodNutrient>? FoodNutrients { get; set; }
        }

        private class FoodNutrient
        {
            public string? NutrientName { get; set; }
            public string? NutrientNumber { get; set; }
            public double? Value { get; set; }
            public string? UnitName { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<CalorieNinjaService> _logger;

        public CalorieNinjaService(
            HttpClient http,
            IConfiguration config,
            ILogger<CalorieNinjaService> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
        {
            var result = new List<NutritionItemDto>();

            if (string.IsNullOrWhiteSpace(query))
                return result;

            try
            {
                var apiKey = _config["CalorieNinjas:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("CalorieNinjas:ApiKey is not configured.");
                    return result; // fail silently, just no suggestions
                }

                var url = $"https://api.calorieninjas.com/v1/nutrition?query={Uri.EscapeDataString(query)}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-Api-Key", apiKey);

                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var parsed = JsonSerializer.Deserialize<CalorieNinjaResponse>(json, options);

                if (parsed?.Items != null)
                {
                    foreach (var item in parsed.Items)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Name))
                        {
                            result.Add(new NutritionItemDto
                            {
                                Name = item.Name,
                                Calories = item.Calories
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CalorieNinjas API");
                // we still return empty list so UI just hides suggestions
            }

            return result;
        }

        // shape of CalorieNinjas response:
        // { "items": [ { "name": "burger", "calories": 295, ... }, ... ] }
        private class CalorieNinjaResponse
        {
            public List<CalorieNinjaItem> Items { get; set; } = new();
        }

        private class CalorieNinjaItem
        {
            public string? Name { get; set; }
            public double Calories { get; set; }
        }
    }
}

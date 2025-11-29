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
        private readonly ILogger<CalorieNinjaService> _logger;
        private readonly string _baseUrl = "https://api.calorieninjas.com/v1/nutrition?query=";

        public CalorieNinjaService(HttpClient http,
                                   ILogger<CalorieNinjaService> logger,
                                   IConfiguration config)
        {
            _http = http;
            _logger = logger;

            var apiKey = config["CalorieNinjas:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                // Ensure header set once
                if (!_http.DefaultRequestHeaders.Contains("X-Api-Key"))
                {
                    _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
                }
            }
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<NutritionItemDto>();

            var url = _baseUrl + Uri.EscapeDataString(query);

            HttpResponseMessage response;
            try
            {
                response = await _http.GetAsync(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CalorieNinjas API.");
                return new List<NutritionItemDto>();
            }

            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "CalorieNinjas API failed. Status: {Status}, Body: {Body}",
                    (int)response.StatusCode, msg);
                return new List<NutritionItemDto>();
            }

            var json = await response.Content.ReadAsStringAsync();

            try
            {
                var items = JsonSerializer.Deserialize<List<NutritionItemDto>>(
                    json,
                    new JsonSerializerOptions
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

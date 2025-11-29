using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CalorieNinjaService> _logger;
        private readonly IConfiguration _config;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CalorieNinjaService(
            HttpClient httpClient,
            ILogger<CalorieNinjaService> logger,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var trimmed = (query ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return new List<NutritionItemDto>();

            var apiKey = _config["NutritionApi:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Nutrition API key not configured (NutritionApi:ApiKey).");
                return new List<NutritionItemDto>();
            }

            var url =
                $"https://api.api-ninjas.com/v1/nutrition?query={Uri.EscapeDataString(trimmed)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Api-Key", apiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Nutrition API failed. Status: {Status}, Body: {Body}",
                    (int)response.StatusCode, body);

                return new List<NutritionItemDto>();
            }

            try
            {
                var items =
                    JsonSerializer.Deserialize<List<NutritionItemDto>>(body, _jsonOptions)
                    ?? new List<NutritionItemDto>();

                return items.FindAll(x => !string.IsNullOrWhiteSpace(x.Name));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Nutrition API response: {Body}", body);
                return new List<NutritionItemDto>();
            }
        }
    }
}

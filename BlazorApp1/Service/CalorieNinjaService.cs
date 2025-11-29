using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlazorApp1.Service
{
    public interface ICalorieNinjaService
    {
        Task<List<NutritionItem>> GetNutritionAsync(string query);
    }

    // This is what your page expects: Name + Calories
    public class NutritionItem
    {
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
    }

    // Raw response shape from CalorieNinjas
    internal class CalorieNinjasResponse
    {
        public List<CalorieNinjasItem> items { get; set; } = new();
    }

    internal class CalorieNinjasItem
    {
        public string name { get; set; } = string.Empty;
        public double calories { get; set; }
    }

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

        public async Task<List<NutritionItem>> GetNutritionAsync(string query)
        {
            var apiKey = _config["CalorieNinjas:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("CalorieNinjas API key is missing (CalorieNinjas:ApiKey).");
                return new List<NutritionItem>();
            }

            var url = $"https://api.calorieninjas.com/v1/nutrition?query={Uri.EscapeDataString(query)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Api-Key", apiKey);

            try
            {
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CalorieNinjas call failed. Status code: {Status}", response.StatusCode);
                    return new List<NutritionItem>();
                }

                var data = await response.Content.ReadFromJsonAsync<CalorieNinjasResponse>();

                if (data?.items == null || data.items.Count == 0)
                    return new List<NutritionItem>();

                var result = new List<NutritionItem>();

                foreach (var i in data.items)
                {
                    result.Add(new NutritionItem
                    {
                        Name = i.name,
                        Calories = i.calories
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CalorieNinjas API.");
                return new List<NutritionItem>();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;

namespace API.Services
{
    public class CaloriesBurnedApiService : ICaloriesBurnedApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<CaloriesBurnedApiService> _logger;
        private readonly string _apiKey;

        // Shape of API Ninjas response
        private class CaloriesApiItem
        {
            public string name { get; set; } = "";
            public double calories_per_hour { get; set; }
            public int duration_minutes { get; set; }
            public double total_calories { get; set; }
        }

        public CaloriesBurnedApiService(
            HttpClient http,
            IConfiguration config,
            ILogger<CaloriesBurnedApiService> logger)
        {
            _http = http;
            _logger = logger;

            _apiKey =
                Environment.GetEnvironmentVariable("APININJAS_API_KEY") ??
                config["ApiNinjas:ApiKey"]
                ?? throw new InvalidOperationException(
                    "API Ninjas key not configured. Set APININJAS_API_KEY or ApiNinjas:ApiKey.");

            if (_http.BaseAddress == null)
            {
                _http.BaseAddress = new Uri("https://api.api-ninjas.com/");
            }

            if (!_http.DefaultRequestHeaders.Contains("X-Api-Key"))
            {
                _http.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
            }
        }

        public async Task<List<ExerciseSuggestion>> SearchExercisesAsync(string query, int? weightKg = null)
        {
            var results = new List<ExerciseSuggestion>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            // API expects pounds for weight
            int? weightLbs = null;
            if (weightKg.HasValue)
            {
                weightLbs = (int)Math.Round(weightKg.Value * 2.20462);
            }

            var encodedQuery = Uri.EscapeDataString(query.Trim());
            var url = $"v1/caloriesburned?activity={encodedQuery}";

            if (weightLbs.HasValue)
                url += $"&weight={weightLbs.Value}";

            try
            {
                var apiItems = await _http.GetFromJsonAsync<List<CaloriesApiItem>>(url)
                              ?? new List<CaloriesApiItem>();

                foreach (var item in apiItems)
                {
                    var perMinute = item.calories_per_hour / 60.0;

                    results.Add(new ExerciseSuggestion
                    {
                        Name = item.name,
                        Category = "", // optional, you can parse or leave blank
                        CaloriesPerMinute = Math.Round(perMinute, 2)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CaloriesBurned API for query {Query}", query);
            }

            return results;
        }
    }
}

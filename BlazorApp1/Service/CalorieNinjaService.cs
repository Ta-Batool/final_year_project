// BlazorApp1/Service/CalorieNinjaService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public CalorieNinjaService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(string query)
        {
            var results = new List<NutritionItemDto>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            var apiKey = _config["CalorieNinjas:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // no key configured – just return empty list
                return results;
            }

            var url =
                $"https://api.calorieninjas.com/v1/nutrition?query={Uri.EscapeDataString(query)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("X-Api-Key", apiKey);

            using var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return results;

            var json = await resp.Content.ReadAsStringAsync();

            var response =
                JsonSerializer.Deserialize<CalorieNinjaResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response?.Items == null || response.Items.Count == 0)
                return results;

            results = response.Items
                .Select(i => new NutritionItemDto
                {
                    Name = i.Name ?? string.Empty,
                    Calories = i.Calories
                })
                .ToList();

            return results;
        }

        // internal classes for deserializing CalorieNinjas JSON
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

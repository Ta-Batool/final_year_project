using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BlazorApp1.Service
{
    public class CalorieNinjaService : ICalorieNinjaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CalorieNinjaService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CalorieNinjaService(HttpClient httpClient, ILogger<CalorieNinjaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<NutritionItemDto>> GetNutritionAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            var results = new List<NutritionItemDto>();

            if (string.IsNullOrWhiteSpace(query))
                return results;

            try
            {
                // Open Food Facts search API
                // Docs: https://world.openfoodfacts.org/data (search API)
                var url =
                    "https://world.openfoodfacts.org/cgi/search.pl" +
                    $"?search_terms={Uri.EscapeDataString(query)}" +
                    "&search_simple=1" +
                    "&action=process" +
                    "&json=1" +
                    "&page_size=10" +
                    "&fields=product_name,nutriments";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                using var response = await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "OpenFoodFacts API failed. Status: {Status}, Body: {Body}",
                        (int)response.StatusCode,
                        await response.Content.ReadAsStringAsync(cancellationToken)
                    );
                    return results;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var offResponse = await JsonSerializer.DeserializeAsync<OpenFoodFactsResponse>(
                    stream,
                    _jsonOptions,
                    cancellationToken
                );

                if (offResponse?.Products == null || offResponse.Products.Count == 0)
                    return results;

                foreach (var p in offResponse.Products)
                {
                    if (string.IsNullOrWhiteSpace(p.ProductName))
                        continue;

                    double? kcal = p.Nutriments?.EnergyKcal100g ?? p.Nutriments?.EnergyKcal;

                    results.Add(new NutritionItemDto
                    {
                        Name = p.ProductName.Trim(),
                        Calories = kcal
                    });
                }

                // keep it small + unique
                results = results
                    .GroupBy(x => x.Name.ToLowerInvariant())
                    .Select(g => g.First())
                    .Take(10)
                    .ToList();

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to parse OpenFoodFacts response for query '{Query}'", query);
                return results;
            }
        }
    }
}

using System.Text.Json.Serialization;

namespace BlazorApp1.Service
{
    // What your Razor page uses
    public class NutritionItemDto
    {
        public string Name { get; set; } = string.Empty;
        public double? Calories { get; set; }   // nullable – OK if missing
    }

    // ------------ Open Food Facts DTOs (internal) ------------

    internal class OpenFoodFactsResponse
    {
        [JsonPropertyName("products")]
        public List<OpenFoodFactsProduct> Products { get; set; } = new();
    }

    internal class OpenFoodFactsProduct
    {
        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [JsonPropertyName("nutriments")]
        public OpenFoodFactsNutriments? Nutriments { get; set; }
    }

    internal class OpenFoodFactsNutriments
    {
        // kcal per 100g – this is what we will use as “calories”
        [JsonPropertyName("energy-kcal_100g")]
        public double? EnergyKcal100g { get; set; }

        // fallback if only generic kcal is present
        [JsonPropertyName("energy-kcal")]
        public double? EnergyKcal { get; set; }
    }
}

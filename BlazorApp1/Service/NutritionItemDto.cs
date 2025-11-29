using System.Text.Json.Serialization;

namespace BlazorApp1.Service
{
    public class NutritionItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("calories")]
        public double? Calories { get; set; }
    }
}

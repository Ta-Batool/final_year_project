using System.Text.Json.Serialization;

namespace BlazorApp1.Service
{
    public class NutritionItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Make nullable so we don't crash if API can't send calories
        [JsonPropertyName("calories")]
        public double? Calories { get; set; }
    }
}

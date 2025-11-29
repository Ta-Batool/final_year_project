using System.Text.Json.Serialization;

namespace BlazorApp1.Service
{
    public class NutritionItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Raw value from API – can be "123.4" or some text
        [JsonPropertyName("calories")]
        public string? CaloriesText { get; set; }

        // Safe numeric value we use in Blazor
        [JsonIgnore]
        public double Calories
        {
            get
            {
                if (double.TryParse(CaloriesText,
                                    System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out var value))
                {
                    return value;
                }

                // Fallback for non-numeric responses
                return 0;
            }
        }
    }
}

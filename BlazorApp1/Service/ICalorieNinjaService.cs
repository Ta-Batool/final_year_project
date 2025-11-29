using System.Text.Json.Serialization;

namespace BlazorApp1.Service
{
    public class NutritionItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Raw value from API – can be "123.4" or "Only available..."
        [JsonPropertyName("calories")]
        public string? CaloriesText { get; set; }

        // Safe numeric value we use everywhere else
        [JsonIgnore]
        public double Calories
        {
            get
            {
                if (double.TryParse(CaloriesText, System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out var value))
                {
                    return value;
                }

                // Fallback if API returns non-numeric text
                return 0;
            }
        }
    }
}

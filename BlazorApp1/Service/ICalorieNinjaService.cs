using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    // Simple DTO that the dashboard will use
    public class NutritionItemDto
    {
        public string Name { get; set; } = string.Empty;

        // May be null if the API does not return calories (free tier etc.)
        public double? Calories { get; set; }
    }

    public interface ICalorieNinjaService
    {
        Task<List<NutritionItemDto>> GetNutritionAsync(string query);
    }
}

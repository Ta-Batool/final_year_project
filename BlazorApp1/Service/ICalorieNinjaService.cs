// BlazorApp1/Service/ICalorieNinjaService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    // Simple DTO used by the dashboard
    public class NutritionItemDto
    {
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
    }

    public interface ICalorieNinjaService
    {
        // Returns a list of food items with calories
        Task<List<NutritionItemDto>> GetNutritionAsync(string query);
    }
}

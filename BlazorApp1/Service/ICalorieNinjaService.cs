using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public class NutritionItemDto
    {
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
    }

    public interface ICalorieNinjaService
    {
        Task<List<NutritionItemDto>> GetNutritionAsync(string query);
    }
}

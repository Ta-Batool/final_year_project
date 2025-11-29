using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface ICalorieNinjaService
    {
        Task<List<NutritionItemDto>> GetNutritionAsync(string query);
    }

    public class NutritionItemDto
    {
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
    }
}

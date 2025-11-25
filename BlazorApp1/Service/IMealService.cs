using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface IMealService
    {
        Task<List<Meal>> GetTodayMealsAsync(string clientId);
        Task AddMealAsync(Meal meal);
        Task DeleteMealAsync(string id);
    }
}

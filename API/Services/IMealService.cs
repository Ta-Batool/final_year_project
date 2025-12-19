using Model;

namespace API.Services
{
    public interface IMealService
    {
        Task<List<Meal>> GetMealsForDayAsync(string clientId, DateTime dateUtc);
        Task<List<Meal>> GetMealsByDateAsync(string clientId, DateTime dateLocal);
        Task<List<Meal>> GetAllForClientAsync(string clientId);
        Task<Meal> CreateAsync(Meal meal);

        Task<int> GetCaloriesForDateAsync(string clientId, DateTime dateUtc);

        Task DeleteAsync(string id);
    }
}

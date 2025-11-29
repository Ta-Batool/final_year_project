using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IMealService
    {
        Task<List<Meal>> GetTodayMealsAsync(string clientId);
        Task<List<Meal>> GetMealsByDateAsync(string clientId, DateTime date);

        Task AddMealAsync(Meal meal);
        Task DeleteMealAsync(string id);
    }
}

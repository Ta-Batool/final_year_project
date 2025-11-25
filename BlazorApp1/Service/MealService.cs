using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class MealService : IMealService
    {
        private readonly HttpClient _http;

        public MService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Meal>> GetTodayMealsAsync(string clientId)
        {
            return await _http.GetFromJsonAsync<List<Meal>>($"api/meals/today/{clientId}")
                   ?? new List<Meal>();
        }

        public async Task AddMealAsync(Meal meal)
        {
            var response = await _http.PostAsJsonAsync("api/meals", meal);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteMealAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/meals/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}

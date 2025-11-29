using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class MealService : IMealService
    {
        private readonly HttpClient _http;

        public MealService(HttpClient http)
        {
            _http = http;
        }

        // Existing: today’s meals (used by other pages, keep it)
        public async Task<List<Meal>> GetTodayMealsAsync(string clientId)
        {
            return await _http.GetFromJsonAsync<List<Meal>>($"api/meals/today/{clientId}")
                   ?? new List<Meal>();
        }

        // ✅ New: meals by specific date (used by functional calendar)
        public async Task<List<Meal>> GetMealsByDateAsync(string clientId, DateTime date)
        {
            // Send date as YYYY-MM-DD (easy to parse on API side)
            var dateString = date.Date.ToString("yyyy-MM-dd");

            var url = $"api/meals/by-date?clientId={clientId}&date={dateString}";
            return await _http.GetFromJsonAsync<List<Meal>>(url)
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

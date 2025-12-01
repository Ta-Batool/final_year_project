using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class ExerciseService : IExerciseService
    {
        private readonly HttpClient _http;

        public ExerciseService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ExerciseLog>> GetTodayAsync(string clientId)
        {
            var result = await _http.GetFromJsonAsync<List<ExerciseLog>>(
                $"api/exercises/today/{clientId}");

            return result ?? new List<ExerciseLog>();
        }

        public async Task<List<ExerciseLog>> GetByDateAsync(string clientId, DateTime date)
        {
            var dateParam = date.ToString("yyyy-MM-dd");
            var url = $"api/exercises/by-date?clientId={clientId}&date={dateParam}";

            var result = await _http.GetFromJsonAsync<List<ExerciseLog>>(url);
            return result ?? new List<ExerciseLog>();
        }

        public async Task AddAsync(ExerciseLog log)
        {
            var response = await _http.PostAsJsonAsync("api/exercises", log);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/exercises/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}

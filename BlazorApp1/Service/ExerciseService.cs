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

        public async Task<List<ExerciseEntry>> GetForDateAsync(string clientId, DateTime date)
        {
            var dateParam = date.ToString("yyyy-MM-dd");
            var result = await _http.GetFromJsonAsync<List<ExerciseEntry>>(
                $"api/exercises/by-date/{clientId}?date={dateParam}");

            return result ?? new List<ExerciseEntry>();
        }

        public async Task AddAsync(ExerciseEntry entry)
        {
            var response = await _http.PostAsJsonAsync("api/exercises", entry);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateStatusAsync(string id, ExerciseStatus status)
        {
            var response = await _http.PatchAsJsonAsync($"api/exercises/{id}/status", status);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/exercises/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}

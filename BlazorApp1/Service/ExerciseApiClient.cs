using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    // This exists ONLY to satisfy older code that registers/uses ExerciseApiClient.
    // Internally it calls the same endpoints your ExerciseService uses.
    public class ExerciseApiClient
    {
        private readonly HttpClient _http;

        public ExerciseApiClient(HttpClient http)
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

        public async Task<List<ExerciseSuggestion>> SearchExercisesAsync(string query, int? weightKg = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<ExerciseSuggestion>();

            var url = $"api/exercises/search?query={Uri.EscapeDataString(query)}";
            if (weightKg.HasValue)
                url += $"&weightKg={weightKg.Value}";

            var result = await _http.GetFromJsonAsync<List<ExerciseSuggestion>>(url);
            return result ?? new List<ExerciseSuggestion>();
        }
    }
}

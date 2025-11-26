using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class MedicationService : IMedicationService
    {
        private readonly HttpClient _http;

        public MedicationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MedicationPlan>> GetPlansAsync(string userId)
        {
            var result = await _http.GetFromJsonAsync<List<MedicationPlan>>(
                $"api/medications/user/{userId}");

            return result ?? new List<MedicationPlan>();
        }

        public async Task AddPlanAsync(MedicationPlan plan)
        {
            var response = await _http.PostAsJsonAsync("api/medications", plan);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeletePlanAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/medications/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<MedicationLog>> GetTodayLogsAsync(string userId)
        {
            var result = await _http.GetFromJsonAsync<List<MedicationLog>>(
                $"api/medications/logs/today/{userId}");

            return result ?? new List<MedicationLog>();
        }

        public async Task UpdateLogStatusAsync(string logId, MedicationStatus status)
        {
            var response = await _http.PostAsync(
                $"api/medications/logs/{logId}/status/{status}",
                null);

            response.EnsureSuccessStatusCode();
        }
    }
}

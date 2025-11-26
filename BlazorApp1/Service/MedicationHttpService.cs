using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class MedicationHttpService : IMedicationHttpService
    {
        private readonly HttpClient _http;

        public MedicationHttpService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MedicationPlan>?> GetPlansAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<MedicationPlan>>(
                $"api/medications/user/{userId}");
        }

        public async Task<MedicationPlan?> CreatePlanAsync(MedicationPlan plan)
        {
            var response = await _http.PostAsJsonAsync("api/medications", plan);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MedicationPlan>();
        }

        public async Task DeletePlanAsync(string planId)
        {
            var response = await _http.DeleteAsync($"api/medications/{planId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<MedicationLog>?> GetTodayLogsAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<MedicationLog>>(
                $"api/medications/logs/today/{userId}");
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

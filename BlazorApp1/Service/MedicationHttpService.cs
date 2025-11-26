using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        // GET /api/medications/user/{userId}
        public async Task<List<MedicationPlan>?> GetPlansAsync(string userId)
        {
            var result = await _http.GetFromJsonAsync<List<MedicationPlan>>(
                $"/api/medications/user/{userId}");

            return result ?? new List<MedicationPlan>();
        }

        // POST /api/medications
        public async Task<MedicationPlan?> CreatePlanAsync(MedicationPlan plan)
        {
            var resp = await _http.PostAsJsonAsync("/api/medications", plan);
            resp.EnsureSuccessStatusCode();

            var created = await resp.Content.ReadFromJsonAsync<MedicationPlan>();
            return created;
        }

        // DELETE /api/medications/{id}
        public async Task DeletePlanAsync(string id)
        {
            var resp = await _http.DeleteAsync($"/api/medications/{id}");
            resp.EnsureSuccessStatusCode();
        }

        // GET /api/medications/logs/today/{userId}
        public async Task<List<MedicationLogDto>?> GetTodayLogsAsync(string userId)
        {
            var result = await _http.GetFromJsonAsync<List<MedicationLogDto>>(
                $"/api/medications/logs/today/{userId}");

            return result ?? new List<MedicationLogDto>();
        }

        // POST /api/medications/logs/{logId}/status/{status}
        public async Task UpdateLogStatusAsync(string logId, string status)
        {
            var resp = await _http.PostAsync(
                $"/api/medications/logs/{logId}/status/{status}",
                content: null);

            resp.EnsureSuccessStatusCode();
        }
    }
}

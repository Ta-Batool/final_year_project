using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    // DTO to match /api/medications/logs/today/{userId}
    public class MedicationLogDto
    {
        public string? Id { get; set; }
        public string PlanId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public interface IMedicationHttpService
    {
        Task<List<MedicationPlan>?> GetPlansAsync(string userId);
        Task<MedicationPlan?> CreatePlanAsync(MedicationPlan plan);
        Task DeletePlanAsync(string id);

        Task<List<MedicationLogDto>?> GetTodayLogsAsync(string userId);
        Task UpdateLogStatusAsync(string logId, string status);
    }
}

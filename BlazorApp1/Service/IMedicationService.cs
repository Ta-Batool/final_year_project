using Model;

namespace BlazorApp1.Service
{
    public interface IMedicationService
    {
        Task<List<MedicationPlan>> GetPlansAsync(string userId);
        Task AddPlanAsync(MedicationPlan plan);
        Task DeletePlanAsync(string id);

        Task<List<MedicationLog>> GetTodayLogsAsync(string userId);
        Task UpdateLogStatusAsync(string logId, MedicationStatus status);
    }
}

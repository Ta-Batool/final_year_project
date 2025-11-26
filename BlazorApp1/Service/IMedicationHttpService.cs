using Model;

namespace BlazorApp1.Service
{
    public interface IMedicationHttpService
    {
        Task<List<MedicationPlan>?> GetPlansAsync(string userId);
        Task<MedicationPlan?> CreatePlanAsync(MedicationPlan plan);
        Task DeletePlanAsync(string planId);
        Task<List<MedicationLog>?> GetTodayLogsAsync(string userId);
        Task UpdateLogStatusAsync(string logId, MedicationStatus status);
    }
}

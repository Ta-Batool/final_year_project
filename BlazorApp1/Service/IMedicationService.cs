using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface IMedicationService
    {
        Task<List<MedicationPlan>> GetPlansAsync(string userId);
        Task<List<MedicationLog>> GetTodayLogsAsync(string userId);
        Task AddPlanAsync(MedicationPlan plan);
        Task DeletePlanAsync(string id);
        Task UpdateLogStatusAsync(string logId, MedicationStatus status);
    }
}

using Model;

namespace API.Services
{
    public interface IExerciseService
    {
        Task<List<ExerciseEntry>> GetForDayAsync(string clientId, DateTime dateUtc);
        Task<ExerciseEntry> AddAsync(ExerciseEntry entry);
        Task UpdateStatusAsync(string id, ExerciseStatus status);
        Task DeleteAsync(string id);
    }
}

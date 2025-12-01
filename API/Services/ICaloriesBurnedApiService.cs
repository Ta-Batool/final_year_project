using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace API.Services
{
    public interface ICaloriesBurnedApiService
    {
        Task<List<ExerciseSuggestion>> SearchExercisesAsync(string query, int? weightKg = null);
    }
}

using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IExercisePlanService
    {
        Task<ExercisePlanResult> BuildPlanAsync(MetabolismSummary meta);
    }
}

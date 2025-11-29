using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BlazorApp1.Service
{
    public interface ICalorieNinjaService
    {
        Task<List<NutritionItemDto>> GetNutritionAsync(
            string query,
            CancellationToken cancellationToken = default);
    }
}

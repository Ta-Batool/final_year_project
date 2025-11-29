using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface ICalorieNinjaService
    {
        Task<List<NutritionItemDto>> GetNutritionAsync(
            string query,
            CancellationToken cancellationToken = default);
    }
}

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class ExercisePlanApiClient : IExercisePlanApiClient
    {
        private readonly HttpClient _http;

        public ExercisePlanApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<ExercisePlanResult?> BuildFromMetabolismAsync(MetabolismSummary meta)
        {
            var res = await _http.PostAsJsonAsync("api/exerciseplans/build", meta);
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<ExercisePlanResult>();
        }
    }

    public interface IExercisePlanApiClient
    {
        Task<ExercisePlanResult?> BuildFromMetabolismAsync(MetabolismSummary meta);
    }
}

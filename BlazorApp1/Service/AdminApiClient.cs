using System.Net.Http.Headers;
using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class AdminApiClient : IAdminApiService
    {
        private readonly HttpClient _http;
        private readonly AdminSession _session;

        public AdminApiClient(HttpClient http, AdminSession session)
        {
            _http = http;
            _session = session;
        }

        private async Task EnsureAuthAsync()
        {
            await _session.LoadAsync();

            if (string.IsNullOrWhiteSpace(_session.BasicAuthHeader))
                throw new Exception("Admin not logged in.");

            _http.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(_session.BasicAuthHeader);
        }

        // Optional - only if you have endpoint in backend
        public async Task<AdminOverviewDto?> GetOverviewAsync()
        {
            await EnsureAuthAsync();

            // If you DON'T have this endpoint, you can delete this method,
            // or keep it and just return null.
            var res = await _http.GetAsync("api/admin/overview");
            if (!res.IsSuccessStatusCode) return null;

            return await res.Content.ReadFromJsonAsync<AdminOverviewDto>();
        }

        public async Task<List<Doctor>> GetPendingDoctorsAsync()
        {
            await EnsureAuthAsync();

            var res = await _http.GetFromJsonAsync<List<Doctor>>("api/admin/doctors/pending");
            return res ?? new List<Doctor>();
        }

        public async Task ReviewDoctorAsync(string doctorId, bool approve, string? notes)
        {
            await EnsureAuthAsync();

            var payload = new { approve, notes };
            var resp = await _http.PostAsJsonAsync($"api/admin/doctors/{doctorId}/review", payload);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"Admin review failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {body}");
            }
        }

        public async Task<List<User>> GetPatientsAsync()
        {
            await EnsureAuthAsync();

            var res = await _http.GetFromJsonAsync<List<User>>("api/admin/patients");
            return res ?? new List<User>();
        }
    }
}

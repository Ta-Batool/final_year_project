using Model;
using System.Net.Http.Json;

namespace BlazorApp1.Service
{
    public class AdminApiClient
    {
        private readonly HttpClient _http;
        private readonly AdminSession _session;

        public AdminApiClient(HttpClient http, AdminSession session)
        {
            _http = http;
            _session = session;
        }

        private void ApplyAuth()
        {
            _http.DefaultRequestHeaders.Remove("Authorization");
            if (!string.IsNullOrWhiteSpace(_session.BasicAuthHeader))
                _http.DefaultRequestHeaders.Add("Authorization", _session.BasicAuthHeader);
        }

        public async Task<List<Doctor>> GetPendingDoctorsAsync()
        {
            ApplyAuth();
            return await _http.GetFromJsonAsync<List<Doctor>>("api/admin/doctors/pending") ?? new();
        }

        public async Task<List<User>> GetPatientsAsync()
        {
            ApplyAuth();
            return await _http.GetFromJsonAsync<List<User>>("api/admin/patients") ?? new();
        }

        public async Task ReviewDoctorAsync(string doctorId, bool approve, string? notes)
        {
            ApplyAuth();
            var resp = await _http.PostAsJsonAsync($"api/admin/doctors/{doctorId}/review",
                new { approve, notes });

            if (!resp.IsSuccessStatusCode)
                throw new Exception(await resp.Content.ReadAsStringAsync());
        }
    }
}

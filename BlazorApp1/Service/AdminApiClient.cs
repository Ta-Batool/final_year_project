using System.Net.Http.Headers;
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

        private async Task EnsureAuthAsync()
        {
            await _session.LoadAsync();

            if (string.IsNullOrWhiteSpace(_session.BasicAuthHeader))
                throw new Exception("Admin not logged in.");

            _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(_session.BasicAuthHeader);
        }

        public async Task<List<Model.Doctor>> GetPendingDoctorsAsync()
        {
            await EnsureAuthAsync();
            var res = await _http.GetFromJsonAsync<List<Model.Doctor>>("api/admin/pending-doctors");
            return res ?? new List<Model.Doctor>();
        }

        public async Task ReviewDoctorAsync(string doctorId, bool approve, string notes)
        {
            await EnsureAuthAsync();
            var payload = new { approve, notes };
            var resp = await _http.PostAsJsonAsync($"api/admin/review-doctor/{doctorId}", payload);
            resp.EnsureSuccessStatusCode();
        }

        public async Task<List<Model.User>> GetPatientsAsync()
        {
            await EnsureAuthAsync();
            var res = await _http.GetFromJsonAsync<List<Model.User>>("api/admin/patients");
            return res ?? new List<Model.User>();
        }
    }
}

using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class AdminApiService : IAdminApiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public AdminApiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        void AddAdminKey()
        {
            var key = _config["ADMIN_API_KEY"];
            _http.DefaultRequestHeaders.Remove("X-ADMIN-KEY");
            if (!string.IsNullOrWhiteSpace(key))
                _http.DefaultRequestHeaders.Add("X-ADMIN-KEY", key);
        }

        public async Task<AdminOverviewDto?> GetOverviewAsync()
        {
            AddAdminKey();
            var raw = await _http.GetFromJsonAsync<dynamic>("api/admin/overview");
            if (raw == null) return null;

            return new AdminOverviewDto
            {
                PendingDoctors = (int)raw.pendingDoctors,
                ApprovedDoctors = (int)raw.approvedDoctors,
                RejectedDoctors = (int)raw.rejectedDoctors,
                Patients = (int)raw.patients
            };
        }

        public async Task<List<Doctor>> GetDoctorsAsync(string status)
        {
            AddAdminKey();
            return await _http.GetFromJsonAsync<List<Doctor>>($"api/admin/doctors?status={status}")
                   ?? new List<Doctor>();
        }

        public async Task<List<User>> GetPatientsAsync()
        {
            AddAdminKey();
            return await _http.GetFromJsonAsync<List<User>>("api/admin/patients")
                   ?? new List<User>();
        }

        public async Task ReviewDoctorAsync(string doctorId, bool approve, string adminClientId, string? notes)
        {
            AddAdminKey();

            var payload = new
            {
                approve,
                adminClientId,
                notes
            };

            var resp = await _http.PostAsJsonAsync($"api/admin/doctors/{doctorId}/review", payload);
            if (!resp.IsSuccessStatusCode)
                throw new Exception(await resp.Content.ReadAsStringAsync());
        }
    }
}

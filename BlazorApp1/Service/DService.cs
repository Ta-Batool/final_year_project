using Model;

namespace BlazorApp1.Service
{
    public class DService : IDService
    {
        private readonly HttpClient _http;

        public DService(HttpClient http)
        {
            _http = http;
        }

        // ✅ Get all doctors
        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _http.GetFromJsonAsync<List<Doctor>>("api/doctors");
        }

        // ✅ Add new doctor
        public async Task AddDoctorAsync(Doctor doctor)
        {
            var response = await _http.PostAsJsonAsync("api/doctors", doctor);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        // ✅ Get doctor by Id
        public async Task<Doctor> GetDoctorByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Doctor>($"api/doctors/{id}");
        }

        // ✅ Update doctor
        public async Task UpdateDoctorAsync(string id, Doctor doctor)
        {
            var response = await _http.PutAsJsonAsync($"api/doctors/{id}", doctor);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public Task DeleteDoctorAsync(string id)
        {
            throw new NotImplementedException();
        }
        public async Task<Doctor> GetDoctorByClientIdAsync(string clientId)
        {
            return await _http.GetFromJsonAsync<Doctor>($"api/doctors/by-client/{clientId}");
        }
        public async Task UpdateDoctorByClientIdAsync(string clientId, Doctor doctor)
        {
            await _http.PutAsJsonAsync($"api/doctors/by-client/{clientId}", doctor);
        }

    }
}

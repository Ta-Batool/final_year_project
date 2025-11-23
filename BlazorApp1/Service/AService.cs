using System.Net.Http;
using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class AService : IAService
    {
        private readonly HttpClient _http;

        public AService(HttpClient http)
        {
            _http = http;
        }

        // ✅ Get all appointments
        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _http.GetFromJsonAsync<List<Appointment>>("api/appointments");
        }

        // ✅ Get appointment by Id
        public async Task<Appointment> GetAppointmentByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Appointment>($"api/appointments/{id}");
        }

        // ✅ Get appointments by user Id
        public async Task<List<Appointment>> GetAppointmentsByUserAsync(string userId)
        {
            return await _http.GetFromJsonAsync<List<Appointment>>($"api/appointments/by-user/{userId}");
        }

        // ✅ Get appointments by doctor Id
        public async Task<List<Appointment>> GetAppointmentsByDoctorAsync(string doctorId)
        {
            return await _http.GetFromJsonAsync<List<Appointment>>($"api/appointments/by-doctor/{doctorId}");
        }

        // ✅ Add new appointment
        public async Task AddAppointmentAsync(Appointment appointment)
        {
            var response = await _http.PostAsJsonAsync("api/appointments", appointment);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        // ✅ Update appointment by Id
        public async Task UpdateAppointmentAsync(string id, Appointment appointment)
        {
            var response = await _http.PutAsJsonAsync($"api/appointments/{id}", appointment);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        // ✅ Delete appointment
        public async Task DeleteAppointmentAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/appointments/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }
        public async Task<bool> UpdateAppointmentStatusAsync(string id, string status)
        {
            var response = await _http.PutAsJsonAsync($"api/appointments/{id}/status", status);
            return response.IsSuccessStatusCode;
        }
    }
}

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _http.GetFromJsonAsync<List<Doctor>>("api/doctors");
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            var response = await _http.PostAsJsonAsync("api/doctors", doctor);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task<Doctor> GetDoctorByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<Doctor>($"api/doctors/{id}");
        }

        public async Task UpdateDoctorAsync(string id, Doctor doctor)
        {
            var response = await _http.PutAsJsonAsync($"api/doctors/{id}", doctor);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task DeleteDoctorAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/doctors/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }

        public async Task<Doctor?> GetDoctorByClientIdAsync(string clientId)
        {
            try
            {
                return await _http.GetFromJsonAsync<Doctor>($"api/doctors/by-client/{clientId}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateDoctorByClientIdAsync(string clientId, Doctor doctor)
        {
            var response = await _http.PutAsJsonAsync($"api/doctors/by-client/{clientId}", doctor);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {error}");
            }
        }
    }
}

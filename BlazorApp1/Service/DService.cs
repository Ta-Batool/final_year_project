using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
            return await _http.GetFromJsonAsync<List<Doctor>>("api/doctors")
                   ?? new List<Doctor>();
        }

        public async Task AddDoctorAsync(Doctor doctor)
        {
            var res = await _http.PostAsJsonAsync("api/doctors", doctor);
            res.EnsureSuccessStatusCode();
        }

        public async Task<Doctor> GetDoctorByIdAsync(string id)
        {
            var doctor = await _http.GetFromJsonAsync<Doctor>($"api/doctors/{id}");
            if (doctor is null)
                throw new InvalidOperationException($"Doctor not found for id: {id}");
            return doctor;
        }

        public async Task UpdateDoctorAsync(string id, Doctor doctor)
        {
            var res = await _http.PutAsJsonAsync($"api/doctors/{id}", doctor);
            res.EnsureSuccessStatusCode();
        }

        public async Task DeleteDoctorAsync(string id)
        {
            var res = await _http.DeleteAsync($"api/doctors/{id}");
            res.EnsureSuccessStatusCode();
        }

        public async Task<Doctor?> GetDoctorByClientIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return null;

            var url = $"api/doctors/by-client/{Uri.EscapeDataString(clientId)}";
            var res = await _http.GetAsync(url);

            if (res.StatusCode == HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();

            return await res.Content.ReadFromJsonAsync<Doctor>();
        }

        public async Task UpdateDoctorByClientIdAsync(string clientId, Doctor doctor)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("clientId is required", nameof(clientId));

            var url = $"api/doctors/by-client/{Uri.EscapeDataString(clientId)}";
            var res = await _http.PutAsJsonAsync(url, doctor);
            res.EnsureSuccessStatusCode();
        }
    }
}

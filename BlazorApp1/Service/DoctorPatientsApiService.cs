using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class DoctorPatientsApiService
    {
        private readonly HttpClient _http;
        public DoctorPatientsApiService(HttpClient http) => _http = http;

        public async Task LinkPatientAsync(string doctorId, string patientUserId)
        {
            var res = await _http.PostAsync($"api/doctors/{doctorId}/patients/{patientUserId}/link", null);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }

        public Task<List<DoctorPatientLink>?> GetDoctorPatientsAsync(string doctorId)
            => _http.GetFromJsonAsync<List<DoctorPatientLink>>($"api/doctors/{doctorId}/patients");

        public async Task PrescribeMedicationAsync(string doctorId, string patientUserId, MedicationPlan plan)
        {
            var res = await _http.PostAsJsonAsync($"api/doctors/{doctorId}/patients/{patientUserId}/medications", plan);
            if (!res.IsSuccessStatusCode)
                throw new Exception(await res.Content.ReadAsStringAsync());
        }
    }
}

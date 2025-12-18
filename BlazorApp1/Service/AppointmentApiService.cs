using System.Net.Http.Json;
using Model;

namespace BlazorApp1.Service
{
    public class AppointmentApiService
    {
        private readonly HttpClient _http;

        public AppointmentApiService(HttpClient http)
        {
            _http = http;
        }

        public Task<AvailabilityResponseDto?> GetAvailabilityAsync(string doctorId, DateTime date, int slotMinutes = 30)
        {
            var url = $"api/appointments/doctor/{doctorId}/availability?date={date:yyyy-MM-dd}&slotMinutes={slotMinutes}";
            return _http.GetFromJsonAsync<AvailabilityResponseDto>(url);
        }

        public async Task<Appointment?> BookAsync(BookAppointmentRequest req)
        {
            var res = await _http.PostAsJsonAsync("api/appointments/book", req);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                throw new Exception($"{(int)res.StatusCode}: {msg}");
            }

            return await res.Content.ReadFromJsonAsync<Appointment>();
        }

        public Task<List<Appointment>?> GetMyAppointmentsAsync(string userId)
        {
            return _http.GetFromJsonAsync<List<Appointment>>($"api/appointments/by-user/{userId}");
        }
    }

    // DTOs (keep these)
    public record TimeSlotDto(DateTime Start, DateTime End);

    public record AvailabilityResponseDto(
        string DoctorId,
        DateTime Date,
        int SlotMinutes,
        List<TimeSlotDto> AvailableSlots
    );

    public record BookAppointmentRequest(
        string PatientUserId,
        string DoctorId,
        DateTime Start,
        int DurationMinutes,
        string? Notes
    );
}

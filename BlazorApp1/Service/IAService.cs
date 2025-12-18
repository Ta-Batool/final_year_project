using Model;

namespace BlazorApp1.Service
{
    public interface IAService
    {
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment> GetAppointmentByIdAsync(string id);
        Task<List<Appointment>> GetAppointmentsByUserAsync(string userId);
        Task<List<Appointment>> GetAppointmentsByDoctorAsync(string doctorId);
        Task AddAppointmentAsync(Appointment appointment);
        Task UpdateAppointmentAsync(string id, Appointment appointment);
        Task DeleteAppointmentAsync(string id);
        Task<bool> UpdateAppointmentStatusAsync(string id, string status);

        Task<AvailabilityResponseDto?> GetAvailabilityAsync(string doctorId, DateTime date, int slotMinutes = 30);
        Task<Appointment?> BookAppointmentAsync(BookAppointmentRequest req);

    }
}

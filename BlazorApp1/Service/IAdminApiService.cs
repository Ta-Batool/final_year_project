using Model;

namespace BlazorApp1.Service
{
    public interface IAdminApiService
    {
        Task<AdminOverviewDto?> GetOverviewAsync();
        Task<List<Doctor>> GetDoctorsAsync(string status);
        Task<List<User>> GetPatientsAsync();
        Task ReviewDoctorAsync(string doctorId, bool approve, string adminClientId, string? notes);
    }

    public class AdminOverviewDto
    {
        public int PendingDoctors { get; set; }
        public int ApprovedDoctors { get; set; }
        public int RejectedDoctors { get; set; }
        public int Patients { get; set; }
    }
}

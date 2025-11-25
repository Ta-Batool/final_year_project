using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorApp1.Service
{
    public interface IDService
    {
        Task<List<Doctor>> GetAllDoctorsAsync();
        Task AddDoctorAsync(Doctor doctor);
        Task<Doctor> GetDoctorByIdAsync(string id);
        Task UpdateDoctorAsync(string id, Doctor doctor);
        Task DeleteDoctorAsync(string id);

        Task<Doctor?> GetDoctorByClientIdAsync(string clientId);
        Task UpdateDoctorByClientIdAsync(string clientId, Doctor doctor);
    }
}

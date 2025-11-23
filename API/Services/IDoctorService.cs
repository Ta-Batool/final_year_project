using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace Services
{
    public interface IDoctorService
    {
        Task<List<Doctor>> GetAllAsync();
        Task<Doctor?> GetByIdAsync(string id);
        Task CreateAsync(Doctor doctor);
        Task UpdateAsync(string id, Doctor doctor);
        Task DeleteAsync(string id);
        Task<Doctor> GetDoctorByClientIdAsync(string clientId);
        Task UpdateDoctorAsync(string id, Doctor doctor);
        Task UpdateDoctorByClientIdAsync(string clientId, Doctor updatedDoctor);
        
    }
}

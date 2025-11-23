using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(string id);
        Task<List<Appointment>> GetByUserIdAsync(string userId);
        Task<List<Appointment>> GetByDoctorIdAsync(string doctorId);
        Task CreateAsync(Appointment appointment);
        Task UpdateAsync(string id, Appointment appointment);
        Task DeleteAsync(string id);
    }
}

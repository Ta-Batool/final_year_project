using Model;
using System;
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

        // 🔹 Used by availability endpoint
        Task<List<Appointment>> GetByDoctorAndDateAsync(string doctorId, DateTime date);

        // 🔹 Used by booking endpoint
        Task<bool> HasOverlapAsync(string doctorId, DateTime start, DateTime end);

        Task CreateAsync(Appointment appointment);
        Task UpdateAsync(string id, Appointment appointment);
        Task DeleteAsync(string id);
    }
}

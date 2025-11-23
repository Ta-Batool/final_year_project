using MongoDB.Driver;
using Model;
using Microsoft.Extensions.Options;
using API.MongoModel; // Your MongoDBSettings class
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IMongoCollection<Appointment> _appointments;

        public AppointmentService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            _appointments = database.GetCollection<Appointment>("Appointment");
        }

        // ✅ Get all appointments
        public async Task<List<Appointment>> GetAllAsync()
        {
            return await _appointments.Find(a => true).ToListAsync();
        }

        // ✅ Get appointment by Id
        public async Task<Appointment?> GetByIdAsync(string id)
        {
            return await _appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        // ✅ Get appointments by User Id
        public async Task<List<Appointment>> GetByUserIdAsync(string userId)
        {
            return await _appointments.Find(a => a.UserId == userId).ToListAsync();
        }

        // ✅ Get appointments by Doctor Id
        public async Task<List<Appointment>> GetByDoctorIdAsync(string doctorId)
        {
            return await _appointments.Find(a => a.DoctorId == doctorId).ToListAsync();
        }

        // ✅ Create new appointment
        public async Task CreateAsync(Appointment appointment)
        {
            await _appointments.InsertOneAsync(appointment);
        }

        // ✅ Update appointment by Id
        public async Task UpdateAsync(string id, Appointment appointment)
        {
            await _appointments.ReplaceOneAsync(a => a.Id == id, appointment);
        }

        // ✅ Delete appointment by Id
        public async Task DeleteAsync(string id)
        {
            await _appointments.DeleteOneAsync(a => a.Id == id);
        }
    }
}

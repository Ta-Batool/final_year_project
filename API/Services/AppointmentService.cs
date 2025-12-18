using MongoDB.Driver;
using Model;
using Microsoft.Extensions.Options;
using API.MongoModel; // Your MongoDBSettings class
using System;
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

        // ✅ NEW: Get all doctor appointments for a specific DATE (used by availability)
        public async Task<List<Appointment>> GetByDoctorAndDateAsync(string doctorId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await _appointments.Find(a =>
                a.DoctorId == doctorId &&
                a.Start >= start &&
                a.Start < end
            ).ToListAsync();
        }

        // ✅ NEW: check if doctor has any overlap in a time range (used by booking)
        public async Task<bool> HasOverlapAsync(string doctorId, DateTime start, DateTime end)
        {
            return await _appointments.Find(a =>
                a.DoctorId == doctorId &&
                a.Status != "Cancelled" &&
                a.Start < end &&
                a.End > start
            ).AnyAsync();
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

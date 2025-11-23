using MongoDB.Driver;
using Model;
using Microsoft.Extensions.Options;
using API.MongoModel;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class DoctorService
    {
        private readonly IMongoCollection<Doctor> _doctors;

        public DoctorService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            // Make sure this matches your collection name in Atlas
            _doctors = database.GetCollection<Doctor>("Doctor");
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await _doctors.Find(d => true).ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(string id)
        {
            return await _doctors.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Doctor doctor)
        {
            await _doctors.InsertOneAsync(doctor);
        }

        public async Task UpdateAsync(string id, Doctor doctor)
        {
            await _doctors.ReplaceOneAsync(d => d.Id == id, doctor);
        }

        public async Task DeleteAsync(string id)
        {
            await _doctors.DeleteOneAsync(d => d.Id == id);
        }

        public async Task<Doctor?> GetDoctorByClientIdAsync(string clientId)
        {
            return await _doctors.Find(d => d.ClientId == clientId).FirstOrDefaultAsync();
        }
        public async Task UpdateDoctorAsync(string id, Doctor doctor)
        {
            await _doctors.ReplaceOneAsync(d => d.Id == id, doctor);
        }

        // ✅ Update doctor by ClientId (optional helper)
        public async Task UpdateDoctorByClientIdAsync(string clientId, Doctor updatedDoctor)
        {
            await _doctors.ReplaceOneAsync(d => d.ClientId == clientId, updatedDoctor);
        }

    }
}

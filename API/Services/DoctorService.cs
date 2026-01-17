using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class DoctorService
    {
        private readonly IMongoCollection<Doctor> _doctors;

        // ✅ FIX: use MongoDBSettings (this is your actual class name)
        public DoctorService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            _doctors = database.GetCollection<Doctor>("Doctor");
        }

        public async Task<List<Doctor>> GetAllAsync() =>
            await _doctors.Find(_ => true).ToListAsync();

        public async Task<Doctor?> GetByIdAsync(string id) =>
            await _doctors.Find(d => d.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Doctor doctor) =>
            await _doctors.InsertOneAsync(doctor);

        public async Task UpdateAsync(string id, Doctor doctor) =>
            await _doctors.ReplaceOneAsync(d => d.Id == id, doctor);

        public async Task DeleteAsync(string id) =>
            await _doctors.DeleteOneAsync(d => d.Id == id);

        public async Task<Doctor?> GetDoctorByClientIdAsync(string clientId) =>
            await _doctors.Find(d => d.ClientId == clientId).FirstOrDefaultAsync();

        // ===========================
        // ✅ Verification helpers
        // ===========================
        public async Task<List<Doctor>> GetByVerificationStatusAsync(DoctorVerificationStatus status) =>
            await _doctors.Find(d => d.VerificationStatus == status).ToListAsync();

        public async Task UpdateCertificateAsync(string doctorId, string fileName, string contentType, string storagePath)
        {
            var update = Builders<Doctor>.Update
                .Set(d => d.CertificateFileName, fileName)
                .Set(d => d.CertificateContentType, contentType)
                .Set(d => d.CertificateStoragePath, storagePath)
                .Set(d => d.CertificateUploadedAt, DateTime.UtcNow)
                .Set(d => d.VerificationStatus, DoctorVerificationStatus.PendingAdminApproval);

            await _doctors.UpdateOneAsync(d => d.Id == doctorId, update);
        }

        public async Task ReviewDoctorAsync(string doctorId, bool approve, string adminId, string? notes)
        {
            var newStatus = approve
                ? DoctorVerificationStatus.Approved
                : DoctorVerificationStatus.Rejected;

            var update = Builders<Doctor>.Update
                .Set(d => d.VerificationStatus, newStatus)
                .Set(d => d.ReviewedByClientId, adminId)
                .Set(d => d.ReviewedAt, DateTime.UtcNow)
                .Set(d => d.ReviewNotes, notes);

            await _doctors.UpdateOneAsync(d => d.Id == doctorId, update);
        }
    }
}

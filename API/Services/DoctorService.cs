using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class DoctorService
    {
        private readonly IMongoCollection<Doctor> _doctors;

        public DoctorService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
            _doctors = mongoDatabase.GetCollection<Doctor>(mongoDbSettings.Value.DoctorsCollectionName);
        }

        public async Task<List<Doctor>> GetAllAsync() =>
            await _doctors.Find(_ => true).ToListAsync();

        public async Task<Doctor?> GetByIdAsync(string id) =>
            await _doctors.Find(d => d.Id == id).FirstOrDefaultAsync();

        public async Task<Doctor?> GetDoctorByClientIdAsync(string clientId) =>
            await _doctors.Find(d => d.ClientId == clientId).FirstOrDefaultAsync();

        public async Task CreateAsync(Doctor doctor) =>
            await _doctors.InsertOneAsync(doctor);

        public async Task UpdateAsync(string id, Doctor updatedDoctor) =>
            await _doctors.ReplaceOneAsync(d => d.Id == id, updatedDoctor);

        public async Task DeleteAsync(string id) =>
            await _doctors.DeleteOneAsync(d => d.Id == id);

        // ===========================
        // ✅ Verification Features
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
            var status = approve ? DoctorVerificationStatus.Approved : DoctorVerificationStatus.Rejected;

            var update = Builders<Doctor>.Update
                .Set(d => d.VerificationStatus, status)
                .Set(d => d.ReviewedByClientId, adminId)
                .Set(d => d.ReviewedAt, DateTime.UtcNow)
                .Set(d => d.ReviewNotes, notes);

            await _doctors.UpdateOneAsync(d => d.Id == doctorId, update);
        }
    }
}

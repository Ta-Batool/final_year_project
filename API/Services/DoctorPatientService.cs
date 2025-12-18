using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class DoctorPatientService
    {
        private readonly IMongoCollection<DoctorPatientLink> _links;

        public DoctorPatientService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _links = database.GetCollection<DoctorPatientLink>("DoctorPatientLinks");
        }

        public Task<List<DoctorPatientLink>> GetDoctorPatientsAsync(string doctorId)
            => _links.Find(x => x.DoctorId == doctorId && x.IsActive).ToListAsync();

        public Task<bool> IsLinkedAsync(string doctorId, string patientUserId)
            => _links.Find(x => x.DoctorId == doctorId && x.PatientUserId == patientUserId && x.IsActive).AnyAsync();

        public Task LinkAsync(string doctorId, string patientUserId)
            => _links.InsertOneAsync(new DoctorPatientLink { DoctorId = doctorId, PatientUserId = patientUserId });

        public async Task UnlinkAsync(string doctorId, string patientUserId)
        {
            var update = Builders<DoctorPatientLink>.Update.Set(x => x.IsActive, false);
            await _links.UpdateOneAsync(
                x => x.DoctorId == doctorId && x.PatientUserId == patientUserId,
                update
            );
        }
    }
}

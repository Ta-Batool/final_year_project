using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class DoctorPatientLink
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string DoctorId { get; set; } = "";

        [BsonRepresentation(BsonType.ObjectId)]
        public string PatientUserId { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    }
}

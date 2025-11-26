using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public enum MedicationStatus
    {
        Upcoming,
        Taken,
        Missed
    }

    public class MedicationLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string MedicationPlanId { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }

        public MedicationStatus Status { get; set; } = MedicationStatus.Upcoming;
    }
}

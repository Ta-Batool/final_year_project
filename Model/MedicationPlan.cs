using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class MedicationPlan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        // e.g. "Metformin"
        public string Name { get; set; } = string.Empty;

        // e.g. "500 mg after breakfast"
        public string Dosage { get; set; } = string.Empty;

        // e.g. "08:00", "20:00"
        public string TimeOfDay { get; set; } = string.Empty;

        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime? EndDate { get; set; }

        public string? Notes { get; set; }
    }
}

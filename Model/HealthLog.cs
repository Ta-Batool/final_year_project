using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class HealthLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = "";

        public double Systolic { get; set; }
        public double Diastolic { get; set; }
        public double Glucose { get; set; }
        public double WeightKg { get; set; }
        public double HeightCm { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

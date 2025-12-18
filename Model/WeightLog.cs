using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class WeightLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = "";
        public double WeightKg { get; set; }
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}

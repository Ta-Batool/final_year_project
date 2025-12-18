using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class GlucoseLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string UserId { get; set; } = "";
        public double ValueMgDl { get; set; }
        public string Type { get; set; } = "Fasting"; // Fasting / Random / PostMeal
        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
    }
}

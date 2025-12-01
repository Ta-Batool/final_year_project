using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class ExerciseLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Reference to the client's Id (same style as Meal.ClientId)
        [BsonRepresentation(BsonType.ObjectId)]
        public string ClientId { get; set; } = null!;

        // e.g. "Morning walk"
        public string Name { get; set; } = string.Empty;

        // e.g. "Cardio", "Strength", "Yoga"
        public string Type { get; set; } = "Cardio";

        // Duration in minutes
        public int DurationMinutes { get; set; }

        // e.g. "Low", "Medium", "High"
        public string Intensity { get; set; } = "Medium";

        // Optional calories burned (for net calories)
        public int? CaloriesBurned { get; set; }

        // Date of exercise (day)
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}

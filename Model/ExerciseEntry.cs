using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExerciseStatus
    {
        Pending = 0,
        Done = 1,
        Skipped = 2
    }

    public class ExerciseEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Link to Client (same as Meal.ClientId)
        [BsonRepresentation(BsonType.ObjectId)]
        public string ClientId { get; set; } = null!;

        // e.g. "Walking", "Push-ups", "Yoga"
        public string Name { get; set; } = string.Empty;

        // 🔹 Use Type instead of Category to match ExerciseLog
        public string Type { get; set; } = "Cardio";

        // Duration in minutes (optional for DB)
        public int? DurationMinutes { get; set; }

        // e.g. "Low", "Medium", "High"
        public string Intensity { get; set; } = "Medium";

        // Optional calories burned – so we persist what Blazor sends
        public int? CaloriesBurned { get; set; }

        // Date this exercise belongs to (only date part is used)
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;

        public ExerciseStatus Status { get; set; } = ExerciseStatus.Pending;

        public string? Notes { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

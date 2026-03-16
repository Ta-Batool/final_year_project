using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class DailyCheckIn
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ClientId { get; set; } = "";
        public DateTime DateUtc { get; set; } = DateTime.UtcNow.Date;

        public double WeightKg { get; set; }
        public double HeightCm { get; set; }

        public int Steps { get; set; }
        public int ExerciseMinutes { get; set; }
        public string FoodNotes { get; set; } = "";
        public string ExerciseNotes { get; set; } = "";

        public int SleepHours { get; set; }
        public int WaterCups { get; set; }
    }
}
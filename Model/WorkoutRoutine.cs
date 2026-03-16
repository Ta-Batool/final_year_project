using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class WorkoutRoutineItem
    {
        public string ExerciseId { get; set; } = "";
        public string ExerciseName { get; set; } = ""; // denormalized for faster UI
        public int Sets { get; set; }
        public int Reps { get; set; }
    }

    public class WorkoutRoutine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = "";
        public string Level { get; set; } = "Beginner"; // Beginner/Intermediate/Advanced
        public string Goal { get; set; } = "Fat Loss";  // Fat Loss/Muscle/Strength
        public List<WorkoutRoutineItem> Items { get; set; } = new();
    }
}

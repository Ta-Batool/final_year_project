using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class WorkoutExercise
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string TargetMuscle { get; set; } = "";
    }
}

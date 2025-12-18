using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Model
{
    public class ExercisePlan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = "";

        public DateTime Date { get; set; }

        public List<PlannedExercise> Items { get; set; } = new();
    }

    public class PlannedExercise
    {
        public string Name { get; set; } = "";
        public int DurationMinutes { get; set; }
        public string? VideoUrl { get; set; }
    }
}

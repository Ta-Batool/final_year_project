using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Model
{
    public class DailyChecklist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = "";

        public DateTime Date { get; set; }

        public Dictionary<string, string> MealStatus { get; set; } = new();     // "done"/"skipped"
        public Dictionary<string, string> ExerciseStatus { get; set; } = new(); // "done"/"skipped"
    }
}

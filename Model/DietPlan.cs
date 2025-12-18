using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Model
{
    public class DietPlan
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = "";

        public DateTime Date { get; set; } // day plan
        public double CalorieTarget { get; set; }

        public List<PlannedMeal> Meals { get; set; } = new();
    }

    public class PlannedMeal
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public double Calories { get; set; }
    }
}

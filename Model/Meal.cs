using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class Meal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Reference to Client collection
        [BsonRepresentation(BsonType.ObjectId)]
        public string ClientId { get; set; } = null!;

        // Breakfast / Lunch / Dinner / Snack
        public string Type { get; set; } = null!;

        // Free text: "2 eggs, toast, tea"
        public string Foods { get; set; } = null!;

        // Optional calories
        public int? Calories { get; set; }

        // Date of the meal (we’ll save only the date part)
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }
}

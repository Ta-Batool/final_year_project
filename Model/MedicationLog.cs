using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MedicationStatus
    {
        Pending = 0,
        Taken = 1,
        Skipped = 2
    }

    public class MedicationLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string PlanId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>UTC date-only (no time)</summary>
        public DateTime Date { get; set; }

        public MedicationStatus Status { get; set; } = MedicationStatus.Pending;

        public string? Notes { get; set; }
    }
}

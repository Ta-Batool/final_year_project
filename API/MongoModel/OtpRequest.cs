using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.MongoModel
{
    public class OtpRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Phone { get; set; } = "";

        // Store hash, not raw OTP
        public string CodeHash { get; set; } = "";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? UsedAtUtc { get; set; }
        public int FailedAttempts { get; set; } = 0;

        // For resend/rate limiting
        public DateTime LastSentAtUtc { get; set; } = DateTime.UtcNow;
    }
}

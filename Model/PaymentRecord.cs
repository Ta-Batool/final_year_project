using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class PaymentRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ClientId { get; set; } = "";
        public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;

        // dummy gateway details (do NOT store full card)
        public string CardLast4 { get; set; } = "";
        public string TransactionRef { get; set; } = "";
        public int AmountPkr { get; set; } = 0;

        public string Status { get; set; } = "Success"; // Success/Failed
    }
}

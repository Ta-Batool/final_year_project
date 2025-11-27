using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace Model
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }       // Unique message ID

        public string? SenderId { get; set; } = "";
        public string? SenderName { get; set; } = "";

        public string? ReceiverId { get; set; } = "";     // for 1-1; can be null/empty in group
        public string? ReceiverName { get; set; } = "";

        public string? UserClientId { get; set; } = "";
        public string? DoctorClientId { get; set; } = "";

        public string? Text { get; set; } = "";
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // 🔹 Attachments (already added earlier)
        public string? AttachmentFileName { get; set; }
        public string? AttachmentContentType { get; set; }
        public byte[]? AttachmentData { get; set; }
        public bool IsVoiceMessage { get; set; } = false;

        // 🔹 NEW: group chat
        public string? ConversationId { get; set; }
    }
}

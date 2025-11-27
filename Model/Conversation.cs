using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Model
{
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        public string Name { get; set; } = "";              // e.g. "Diabetes Care Group"
        public bool IsGroup { get; set; } = true;           // for future: 1-1 vs group

        // List of ClientIds (doctors + patients)
        public List<string> ParticipantIds { get; set; } = new();

        public string? CreatedByClientId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // For simple video call integration (Jitsi / Meet link)
        public string? MeetingUrl { get; set; }
    }
}

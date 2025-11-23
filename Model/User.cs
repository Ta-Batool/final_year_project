using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        public string? ClientId { get; set; }

        public string? Name { get; set; } = "";

        // 🔹 Added to match FormUser.razor
        public string? Email { get; set; } = "";

        // 🔹 You can keep both Sex and Gender for now
        public string? Gender { get; set; } = "";

        // ⚠️ In a real app you’d hash this, but for now we just match the form
        public string? Password { get; set; } = "";

        public int Age { get; set; }   // 🔹 Added

        public string? Reason { get; set; } = "";
        public string? Time { get; set; } = "";
        public string? Address { get; set; } = "";
        public DateTime DOB { get; set; }
        public string? Sex { get; set; } = "";
        public string? Weight { get; set; } = "";
        public string? Height { get; set; } = "";
        public DateTime LastAppointment { get; set; }
        public DateTime RegisterDate { get; set; }
        public string? Phone { get; set; } = "";
        public List<string> Tags { get; set; } = new();
    }
}

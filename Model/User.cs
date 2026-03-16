using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        public string? ClientId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; } = "";

        // 🔹 Added to match FormUser.razor
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        public string? Email { get; set; } = "";

        // 🔹 You can keep both Sex and Gender for now
        [Required(ErrorMessage = "Gender is required.")]
        public string? Gender { get; set; } = "";

        // ⚠️ In a real app you’d hash this, but for now we just match the form
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; } = "";

        [Range(10, 120, ErrorMessage = "Age must be between 10 and 120.")]
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

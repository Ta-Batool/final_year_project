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

        public string? GoogleId { get; set; } = "";

        public string? AuthProvider { get; set; } = "Google";

        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        public string? Email { get; set; } = "";

        [Required(ErrorMessage = "Gender is required.")]
        public string? Gender { get; set; } = "";

        [Range(10, 120, ErrorMessage = "Age must be between 10 and 120.")]
        public int Age { get; set; }

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
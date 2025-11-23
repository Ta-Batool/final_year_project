using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Appointment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientName { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
        public string? Notes { get; set; } = "";
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Model
{
    public class Doctor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        public string? ClientId { get; set; } 
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public int Age { get; set; }
        public string? Specialization { get; set; }
        public string? Department { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PMDCNumber {  get; set; }

        // Auto-calculate experience years from JoiningDate
        public DateTime? StartDate { get; set; }   // Doctor’s first job start date
        
        public int ExperienceYears { get; set; }
        //{
            //get
            //{
            //    if (StartDate.HasValue)
            //    {
            //        var today = DateTime.Today;
            //        int years = today.Year - StartDate.Value.Year;

            //        // If current date is before the anniversary of StartDate, subtract 1 year
            //        if (today < StartDate.Value.AddYears(years))
            //            years--;

            //        return years;
            //    }
            //    return 0;
            //}
        //}


        public double ConsultationFee { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public string? Qualification { get; set; }
        public List<string>? Languages { get; set; }

        // Workplace info flattened
        public string? WorkplaceType { get; set; }
        public string? WorkplaceName { get; set; }
        public string? WorkplaceAddress { get; set; }
        public string? WorkplaceContact { get; set; }
        public bool IsPrimaryWorkplace { get; set; }

        // Available days and slots (flattened as lists)
        public List<TimeSlots> Slots { get; set; } = new List<TimeSlots>();
        public List<string>? SlotModes { get; set; }

        public string? WorkplaceNotes { get; set; }

        public bool IsActive { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? ProfileImageUrl { get; set; }
        public Dictionary<string, string>? AdditionalInfo { get; set; }

    }

    public class TimeSlots
    {
        public string? Day { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
    }

   
}

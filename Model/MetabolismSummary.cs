using System;

namespace Model
{
    public class MetabolismSummary
    {
        // Profile
        public double WeightKg { get; set; }
        public double HeightCm { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = "Male";

        // Core metabolism
        public int Bmr { get; set; }
        public int MaintenanceCalories { get; set; }

        // Daily stats
        public int CaloriesConsumed { get; set; }
        public int CaloriesBurned { get; set; }

        // Derived
        public int NetCalories { get; set; }          // Consumed - Burned
        public int DeficitOrSurplus { get; set; }     // Net - Maintenance
    }
}

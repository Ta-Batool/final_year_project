namespace Model
{
    public class MetabolismSummary
    {
        public double WeightKg { get; set; }
        public double HeightCm { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = "Male";

        public int Bmr { get; set; }
        public int MaintenanceCalories { get; set; }

        public int CaloriesConsumed { get; set; }
        public int CaloriesBurned { get; set; }

        public int NetCalories => CaloriesConsumed - CaloriesBurned;

        public string GoalStatus { get; set; } = "";
        public int DeficitOrSurplus => NetCalories - MaintenanceCalories; 
        // negative => deficit, positive => surplus
    }
}

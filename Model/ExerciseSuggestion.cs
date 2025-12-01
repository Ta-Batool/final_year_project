namespace Model
{
    public class ExerciseSuggestion
    {
        public string Name { get; set; } = string.Empty;

        // Category is optional; for Ninjas we can leave it empty or derive from name
        public string Category { get; set; } = string.Empty;

        // Calculated from calories_per_hour / 60
        public double CaloriesPerMinute { get; set; }
    }
}

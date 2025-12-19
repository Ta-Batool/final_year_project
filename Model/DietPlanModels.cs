using System.Collections.Generic;

namespace Model
{
    public class DietPlanResult
    {
        public string Title { get; set; } = "Diet Plan";
        public int TargetCalories { get; set; }
        public string Notes { get; set; } = "";

        public List<DietPlanItem> Items { get; set; } = new();
    }

    public class DietPlanItem
    {
        public string Meal { get; set; } = "";        // Breakfast, Lunch, Dinner
        public int Calories { get; set; }
        public string Example { get; set; } = "";     // Example food
    }
}

using System;
using System.Collections.Generic;

namespace Model
{
    public class ExercisePlanResult
    {
        public string Title { get; set; } = "Personalized Exercise Plan";
        public int TargetCaloriesToBurn { get; set; }  // daily suggested burn
        public string Notes { get; set; } = "";
        public List<ExercisePlanItem> Items { get; set; } = new();
    }

    public class ExercisePlanItem
    {
        public string DayLabel { get; set; } = "";           // e.g. "Today" / "Mon"
        public string Type { get; set; } = "Cardio";         // Cardio/Strength/Yoga
        public string Intensity { get; set; } = "Medium";    // Low/Medium/High
        public int Minutes { get; set; } = 30;
        public string Example { get; set; } = "Brisk Walk";
        public int EstimatedCaloriesBurned { get; set; }
    }
}

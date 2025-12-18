using System;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    // Frontend-only plan generator (no DB needed)
    public class ExercisePlanService : IExercisePlanService
    {
        public Task<ExercisePlanResult> BuildPlanAsync(MetabolismSummary meta)
        {
            // basic safety
            if (meta == null || meta.MaintenanceCalories <= 0)
            {
                return Task.FromResult(new ExercisePlanResult
                {
                    Title = "Exercise Plan",
                    Notes = "Complete your profile (age/height/weight) to generate a plan."
                });
            }

            // Goal: if user is already in surplus vs maintenance -> suggest more burn,
            // if already deficit -> light plan, if near maintenance -> balanced
            // DeficitOrSurplus = Net - Maintenance
            // Positive => surplus (ate more than maintenance), negative => deficit

            int surplus = meta.DeficitOrSurplus;
            int targetBurn;

            if (surplus >= 500) targetBurn = 450;
            else if (surplus >= 200) targetBurn = 350;
            else if (surplus >= 0) targetBurn = 250;
            else if (surplus <= -500) targetBurn = 120;
            else targetBurn = 180;

            // calories-per-minute rough estimate using weight and intensity
            // (very simple estimate; good for FYP demo)
            double w = Math.Max(40, meta.WeightKg);

            // helper
            int Burn(int minutes, double factor) => (int)Math.Round(minutes * factor * (w / 70.0));

            // Build a "Today" plan only (smooth integration with your exercise page)
            // You can later expand to 7-day plan.
            var plan = new ExercisePlanResult
            {
                Title = "Today's Personalized Exercise Plan",
                TargetCaloriesToBurn = targetBurn,
                Notes = "Based on your maintenance calories + today's intake/exercise. Adjust intensity as needed."
            };

            // Choose plan type by surplus
            if (surplus >= 200)
            {
                // Cardio focus
                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Cardio",
                    Intensity = "Medium",
                    Minutes = 35,
                    Example = "Brisk walk / Cycling",
                    EstimatedCaloriesBurned = Burn(35, factor: 8.0)
                });

                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Strength",
                    Intensity = "Low",
                    Minutes = 20,
                    Example = "Bodyweight circuit",
                    EstimatedCaloriesBurned = Burn(20, factor: 6.0)
                });
            }
            else
            {
                // Balanced / recovery
                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Cardio",
                    Intensity = "Low",
                    Minutes = 25,
                    Example = "Easy walk",
                    EstimatedCaloriesBurned = Burn(25, factor: 6.0)
                });

                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Yoga",
                    Intensity = "Low",
                    Minutes = 15,
                    Example = "Stretching / Mobility",
                    EstimatedCaloriesBurned = Burn(15, factor: 4.0)
                });
            }

            return Task.FromResult(plan);
        }
    }
}

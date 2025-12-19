using System;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class ExercisePlanService : IExercisePlanService
    {
        public Task<ExercisePlanResult> BuildPlanAsync(MetabolismSummary meta)
        {
            if (meta == null || meta.MaintenanceCalories <= 0)
            {
                return Task.FromResult(new ExercisePlanResult
                {
                    Title = "Exercise Plan",
                    Notes = "Complete your profile to generate a plan."
                });
            }

            int surplus = meta.DeficitOrSurplus;
            int targetBurn;

            if (surplus >= 500) targetBurn = 450;
            else if (surplus >= 200) targetBurn = 350;
            else if (surplus >= 0) targetBurn = 250;
            else if (surplus <= -500) targetBurn = 120;
            else targetBurn = 180;

            double w = Math.Max(40, meta.WeightKg);
            int Burn(int minutes, double factor)
                => (int)Math.Round(minutes * factor * (w / 70.0));

            var plan = new ExercisePlanResult
            {
                Title = "Today's Personalized Exercise Plan",
                TargetCaloriesToBurn = targetBurn,
                Notes = "Generated from BMR, maintenance calories and today's activity."
            };

            if (surplus >= 200)
            {
                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Cardio",
                    Intensity = "Medium",
                    Minutes = 35,
                    Example = "Brisk walk / Cycling",
                    EstimatedCaloriesBurned = Burn(35, 8.0)
                });

                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Strength",
                    Intensity = "Low",
                    Minutes = 20,
                    Example = "Bodyweight workout",
                    EstimatedCaloriesBurned = Burn(20, 6.0)
                });
            }
            else
            {
                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Cardio",
                    Intensity = "Low",
                    Minutes = 25,
                    Example = "Easy walk",
                    EstimatedCaloriesBurned = Burn(25, 6.0)
                });

                plan.Items.Add(new ExercisePlanItem
                {
                    DayLabel = "Today",
                    Type = "Yoga",
                    Intensity = "Low",
                    Minutes = 15,
                    Example = "Stretching / Mobility",
                    EstimatedCaloriesBurned = Burn(15, 4.0)
                });
            }

            return Task.FromResult(plan);
        }
    }
}

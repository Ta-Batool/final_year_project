using System.Threading.Tasks;
using Model;

namespace API.Services
{
    public class DietPlanService
    {
        public Task<DietPlanResult> GeneratePlanAsync(MetabolismSummary metabolism)
        {
            if (metabolism == null || metabolism.MaintenanceCalories <= 0)
            {
                return Task.FromResult(new DietPlanResult
                {
                    Title = "Diet Plan",
                    Notes = "Complete your profile to generate a personalized diet plan."
                });
            }

            int targetCalories;

            if (metabolism.DeficitOrSurplus > 300)
                targetCalories = metabolism.MaintenanceCalories - 300;
            else if (metabolism.DeficitOrSurplus < -300)
                targetCalories = metabolism.MaintenanceCalories + 200;
            else
                targetCalories = metabolism.MaintenanceCalories;

            var plan = new DietPlanResult
            {
                Title = "Today's Personalized Diet Plan",
                TargetCalories = targetCalories,
                Notes = "Calories distributed across meals."
            };

            plan.Items.Add(new DietPlanItem
            {
                Meal = "Breakfast",
                Calories = (int)(targetCalories * 0.30),
                Example = "Eggs, toast, fruit"
            });

            plan.Items.Add(new DietPlanItem
            {
                Meal = "Lunch",
                Calories = (int)(targetCalories * 0.40),
                Example = "Chicken, rice, vegetables"
            });

            plan.Items.Add(new DietPlanItem
            {
                Meal = "Dinner",
                Calories = (int)(targetCalories * 0.25),
                Example = "Fish or lentils with salad"
            });

            plan.Items.Add(new DietPlanItem
            {
                Meal = "Snack",
                Calories = (int)(targetCalories * 0.05),
                Example = "Yogurt or nuts"
            });

            return Task.FromResult(plan);
        }
    }
}

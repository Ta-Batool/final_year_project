using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using API.MongoModel;
using Model;

namespace API.Services
{
    public class DietPlanService
    {
        private readonly IMongoCollection<DietPlan> _plans;

        public DietPlanService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            _plans = database.GetCollection<DietPlan>("DietPlan");
        }

        // -------------------------
        // CRUD used by DietPlansController
        // -------------------------

        public async Task<DietPlan?> GetByUserAndDateAsync(string userId, DateTime date)
        {
            var day = date.Date;
            return await _plans.Find(p => p.UserId == userId && p.Date == day).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(DietPlan plan)
        {
            plan.Date = plan.Date.Date;
            await _plans.InsertOneAsync(plan);
        }

        public async Task UpdateAsync(string id, DietPlan plan)
        {
            plan.Id = id;
            plan.Date = plan.Date.Date;

            await _plans.ReplaceOneAsync(p => p.Id == id, plan);
        }

        // -------------------------
        // Your existing generator
        // -------------------------

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

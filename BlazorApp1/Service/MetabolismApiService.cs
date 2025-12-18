using System;
using System.Linq;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public class MetabolismApiService
    {
        private readonly IUService _userService;
        private readonly IMealService _mealService;
        private readonly IExerciseService _exerciseService;

        public MetabolismApiService(
            IUService userService,
            IMealService mealService,
            IExerciseService exerciseService)
        {
            _userService = userService;
            _mealService = mealService;
            _exerciseService = exerciseService;
        }

        public async Task<MetabolismSummary> GetSummaryAsync(string clientId)
        {
            // Get profile
            var user = await _userService.GetUserByClientIdAsync(clientId);

            // Fallback-safe defaults if profile missing fields
            double weightKg = 70;
            double heightCm = 170;
            int age = 25;
            string gender = "Male";

            if (user != null)
            {
                // ⚠️ IMPORTANT: adjust these property names if your User model differs.
                // If your model uses Height/Weight/Dob etc, tell me and I will map 1:1.
                try { if (user.WeightKg > 0) weightKg = user.WeightKg; } catch { }
                try { if (user.HeightCm > 0) heightCm = user.HeightCm; } catch { }
                try { if (user.Age > 0) age = user.Age; } catch { }
                try
                {
                    if (!string.IsNullOrWhiteSpace(user.Gender))
                        gender = user.Gender;
                }
                catch { }
            }

            // Meals + exercise today
            var meals = await _mealService.GetTodayMealsAsync(clientId) ?? new();
            var exLogs = await _exerciseService.GetTodayAsync(clientId) ?? new();

            int consumed = meals.Sum(m => m.Calories ?? 0);
            int burned = exLogs.Sum(e => e.CaloriesBurned ?? 0);

            // ✅ Mifflin–St Jeor BMR
            int bmr = gender.Equals("female", StringComparison.OrdinalIgnoreCase)
                ? (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age - 161)
                : (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age + 5);

            // ✅ Maintenance (activity factor – pick moderate for now)
            int maintenance = (int)Math.Round(bmr * 1.55);

            return new MetabolismSummary
            {
                Bmr = bmr,
                MaintenanceCalories = maintenance,
                CaloriesConsumed = consumed,
                CaloriesBurned = burned
            };
        }
    }
}

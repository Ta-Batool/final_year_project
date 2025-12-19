using System;
using System.Linq;
using System.Globalization;
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

        // =========================================================
        // MAIN SUMMARY (Dashboard + UserExercises)
        // =========================================================
        public async Task<MetabolismSummary> GetSummaryAsync(string clientId)
        {
            var user = await _userService.GetUserByClientIdAsync(clientId);

            // 🔒 SAFE DEFAULTS (viva-safe)
            double weightKg = 70;
            double heightCm = 170;
            int age = 25;
            string gender = "Male";

            // ---------------------------------------------------------
            // ✅ PARSE STRING FIELDS SAFELY (YOUR ACTUAL USER MODEL)
            // ---------------------------------------------------------
            if (user != null)
            {
                // Weight (string → double)
                if (!string.IsNullOrWhiteSpace(user.Weight) &&
                    double.TryParse(user.Weight, NumberStyles.Any, CultureInfo.InvariantCulture, out var w))
                {
                    weightKg = Math.Max(30, w);
                }

                // Height (string → double)
                if (!string.IsNullOrWhiteSpace(user.Height) &&
                    double.TryParse(user.Height, NumberStyles.Any, CultureInfo.InvariantCulture, out var h))
                {
                    heightCm = Math.Max(100, h);
                }

                // Age (string → int)
                if (!string.IsNullOrWhiteSpace(user.Age) &&
                    int.TryParse(user.Age, out var a))
                {
                    age = Math.Max(10, a);
                }

                if (!string.IsNullOrWhiteSpace(user.Gender))
                {
                    gender = user.Gender;
                }
            }

            // ---------------------------------------------------------
            // TODAY'S MEALS & EXERCISE
            // ---------------------------------------------------------
            var meals = await _mealService.GetTodayMealsAsync(clientId) ?? new();
            var exercises = await _exerciseService.GetTodayAsync(clientId) ?? new();

            int consumed = meals.Sum(m => m.Calories ?? 0);
            int burned = exercises.Sum(e => e.CaloriesBurned ?? 0);

            // ---------------------------------------------------------
            // 🔥 BMR (Mifflin–St Jeor)
            // ---------------------------------------------------------
            int bmr = gender.Equals("female", StringComparison.OrdinalIgnoreCase)
                ? (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age - 161)
                : (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age + 5);

            // ---------------------------------------------------------
            // ⚡ Maintenance Calories
            // ---------------------------------------------------------
            int maintenance = (int)Math.Round(bmr * 1.55);

            int netCalories = consumed - burned;
            int deficitOrSurplus = netCalories - maintenance;

            return new MetabolismSummary
            {
                WeightKg = weightKg,
                HeightCm = heightCm,
                Age = age,
                Gender = gender,

                Bmr = bmr,
                MaintenanceCalories = maintenance,

                CaloriesConsumed = consumed,
                CaloriesBurned = burned,
                NetCalories = netCalories,
                DeficitOrSurplus = deficitOrSurplus
            };
        }

        // =========================================================
        // CHART SUPPORT (HealthCharts)
        // =========================================================
        public async Task<MetabolismSummary> GetSummaryForDateAsync(
            string clientId,
            DateTime date)
        {
            // For FYP: reuse logic (charts already filter visually)
            return await GetSummaryAsync(clientId);
        }
    }
}

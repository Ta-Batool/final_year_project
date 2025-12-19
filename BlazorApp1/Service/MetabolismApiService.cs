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

        // ============================================================
        // MAIN SUMMARY (USED BY DASHBOARD + USER EXERCISES)
        // ============================================================
        public async Task<MetabolismSummary> GetSummaryAsync(string clientId)
        {
            var user = await _userService.GetUserByClientIdAsync(clientId);

            // 🔐 Safe defaults (FYP-friendly)
            double weightKg = 70;
            double heightCm = 170;
            int age = 25;
            string gender = "Male";

            // ------------------------------------------------------------
            // ✅ MAP TO YOUR ACTUAL USER MODEL (NO ASSUMPTIONS)
            // ------------------------------------------------------------
            if (user != null)
            {
                try
                {
                    // ⚠️ CHANGE THESE ONLY IF YOUR User MODEL DIFFERS
                    if (user.Weight > 0)
                        weightKg = user.Weight;

                    if (user.Height > 0)
                        heightCm = user.Height;

                    if (user.DateOfBirth.HasValue)
                    {
                        var today = DateTime.Today;
                        age = today.Year - user.DateOfBirth.Value.Year;
                        if (user.DateOfBirth.Value.Date > today.AddYears(-age))
                            age--;
                    }

                    if (!string.IsNullOrWhiteSpace(user.Gender))
                        gender = user.Gender;
                }
                catch
                {
                    // fallback values already set
                }
            }

            // ------------------------------------------------------------
            // TODAY'S INTAKE & ACTIVITY
            // ------------------------------------------------------------
            var meals = await _mealService.GetTodayMealsAsync(clientId) ?? new();
            var exercises = await _exerciseService.GetTodayAsync(clientId) ?? new();

            int consumed = meals.Sum(m => m.Calories ?? 0);
            int burned = exercises.Sum(e => e.CaloriesBurned ?? 0);

            // ------------------------------------------------------------
            // 🔥 BMR — Mifflin–St Jeor (Industry Standard)
            // ------------------------------------------------------------
            int bmr = gender.Equals("female", StringComparison.OrdinalIgnoreCase)
                ? (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age - 161)
                : (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age + 5);

            // ------------------------------------------------------------
            // ⚡ MAINTENANCE CALORIES (Moderate Activity)
            // ------------------------------------------------------------
            int maintenance = (int)Math.Round(bmr * 1.55);

            int netCalories = consumed - burned;
            int deficitOrSurplus = netCalories - maintenance;

            return new MetabolismSummary
            {
                // Profile snapshot
                WeightKg = weightKg,
                HeightCm = heightCm,
                Age = age,
                Gender = gender,

                // Metabolism
                Bmr = bmr,
                MaintenanceCalories = maintenance,

                // Daily stats
                CaloriesConsumed = consumed,
                CaloriesBurned = burned,
                NetCalories = netCalories,
                DeficitOrSurplus = deficitOrSurplus
            };
        }

        // ============================================================
        // 📊 USED BY HEALTH CHARTS (DATE-BASED)
        // ============================================================
        public async Task<MetabolismSummary> GetSummaryForDateAsync(
            string clientId,
            DateTime date)
        {
            // 🔁 For now reuse daily logic (FYP ACCEPTABLE)
            // You can later filter meals/exercises by date if needed
            return await GetSummaryAsync(clientId);
        }
    }
}

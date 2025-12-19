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
            var user = await _userService.GetUserByClientIdAsync(clientId);

            // ✅ safe defaults
            double weightKg = 70;
            double heightCm = 170;
            int age = 25;
            string gender = "Male";

            if (user != null)
            {
                // ✅ WEIGHT (try numeric prop first, then string prop)
                TryGetDouble(user, "WeightKg", ref weightKg);
                TryGetDouble(user, "Weight", ref weightKg);
                TryGetDouble(user, "weight", ref weightKg);

                // ✅ HEIGHT
                TryGetDouble(user, "HeightCm", ref heightCm);
                TryGetDouble(user, "Height", ref heightCm);
                TryGetDouble(user, "height", ref heightCm);

                // ✅ AGE
                TryGetInt(user, "Age", ref age);
                TryGetInt(user, "age", ref age);

                // ✅ GENDER
                TryGetString(user, "Gender", ref gender);
                TryGetString(user, "gender", ref gender);
            }

            // Meals + exercise today
            var meals = await _mealService.GetTodayMealsAsync(clientId) ?? new();
            var exLogs = await _exerciseService.GetTodayAsync(clientId) ?? new();

            int consumed = meals.Sum(m => m.Calories ?? 0);
            int burned = exLogs.Sum(e => e.CaloriesBurned ?? 0);

            // ✅ BMR (Mifflin–St Jeor)
            int bmr = gender.Equals("female", StringComparison.OrdinalIgnoreCase)
                ? (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age - 161)
                : (int)Math.Round(10 * weightKg + 6.25 * heightCm - 5 * age + 5);

            int maintenance = (int)Math.Round(bmr * 1.55);

            int net = consumed - burned;
            int deficitOrSurplus = net - maintenance;

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
                NetCalories = net,
                DeficitOrSurplus = deficitOrSurplus
            };
        }

        // ----------------- helpers (reflection-safe) -----------------

        private static void TryGetDouble(object obj, string propName, ref double target)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return;

                var val = p.GetValue(obj);
                if (val == null) return;

                if (val is double d && d > 0) { target = d; return; }
                if (val is float f && f > 0) { target = f; return; }
                if (val is int i && i > 0) { target = i; return; }
                if (val is long l && l > 0) { target = l; return; }

                if (val is string s)
                {
                    s = s.Trim();
                    if (double.TryParse(s, out var parsed) && parsed > 0)
                        target = parsed;
                }
            }
            catch { }
        }

        private static void TryGetInt(object obj, string propName, ref int target)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return;

                var val = p.GetValue(obj);
                if (val == null) return;

                if (val is int i && i > 0) { target = i; return; }
                if (val is long l && l > 0) { target = (int)l; return; }

                if (val is string s)
                {
                    s = s.Trim();
                    if (int.TryParse(s, out var parsed) && parsed > 0)
                        target = parsed;
                }
            }
            catch { }
        }
        
        public Task<MetabolismSummary> GetSummaryForDateAsync(string clientId, DateTime date)
        {
            // Your current frontend services calculate "today" using GetTodayMealsAsync/GetTodayAsync
            // and do not have a date-based method on those services in this Blazor layer.
            // To fix the build and keep behavior consistent, we return the same summary.
            // If later you add GetMealsByDate / GetForDay in the Blazor services, update this.
            return GetSummaryAsync(clientId);
        }

        private static void TryGetString(object obj, string propName, ref string target)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return;

                var val = p.GetValue(obj)?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                    target = val.Trim();
            }
            catch { }
        }
    }
}

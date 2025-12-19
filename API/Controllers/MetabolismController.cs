using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;
using System;
using System.Globalization;
using System.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("api/metabolism")]
    public class MetabolismController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly WeightLogService _weightService;
        private readonly MealService _mealService;
        private readonly ExerciseService _exerciseService;

        public MetabolismController(
            UserService userService,
            WeightLogService weightService,
            MealService mealService,
            ExerciseService exerciseService)
        {
            _userService = userService;
            _weightService = weightService;
            _mealService = mealService;
            _exerciseService = exerciseService;
        }

        // ---------------- SUMMARY ----------------
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetSummary(string userId)
        {
            var user = await _userService.GetByClientIdAsync(userId);
            if (user == null) return NotFound("User not found");

            var latestWeight = await _weightService.GetLatestWeightAsync(userId);
            if (latestWeight == null) return BadRequest("Weight log required");

            // Weight from logs
            double weightKg = latestWeight.WeightKg;

            // Height from user model (often stored as string)
            double heightCm = 170;
            if (!TryGetDouble(user, "HeightCm", ref heightCm) &&
                !TryGetDouble(user, "Height", ref heightCm) &&
                !TryGetDouble(user, "height", ref heightCm))
            {
                // keep default
            }

            // Age might not exist in your User model; default safely
            int age = 25;
            TryGetInt(user, "Age", ref age);
            TryGetInt(user, "age", ref age);

            // Gender might be string; default safely
            string gender = "Male";
            TryGetString(user, "Gender", ref gender);
            TryGetString(user, "gender", ref gender);

            bool isFemale = gender.Equals("female", StringComparison.OrdinalIgnoreCase);

            // BMR (Mifflin–St Jeor)
            double bmr = isFemale
                ? (10 * weightKg) + (6.25 * heightCm) - (5 * age) - 161
                : (10 * weightKg) + (6.25 * heightCm) - (5 * age) + 5;

            // ActivityLevel doesn't exist in your model -> default moderate (1.55)
            double activityFactor = 1.55;
            // If later you add ActivityLevel in User, this reflection will pick it up:
            string activityLevel = "";
            if (TryGetString(user, "ActivityLevel", ref activityLevel) ||
                TryGetString(user, "activityLevel", ref activityLevel))
            {
                activityFactor = activityLevel switch
                {
                    "Sedentary" => 1.2,
                    "Light" => 1.375,
                    "Moderate" => 1.55,
                    "Active" => 1.725,
                    "VeryActive" => 1.9,
                    _ => 1.55
                };
            }

            double maintenance = bmr * activityFactor;

            var today = DateTime.UtcNow.Date;

            // These methods must exist (we added them earlier)
            double consumed = await _mealService.GetCaloriesForDateAsync(userId, today);
            double burned = await _exerciseService.GetCaloriesBurnedForDateAsync(userId, today);

            double net = consumed - burned;
            double delta = net - maintenance;

            return Ok(new
            {
                weightKg,
                heightCm,
                age,
                gender,
                bmr = Math.Round(bmr),
                maintenanceCalories = Math.Round(maintenance),
                caloriesConsumed = Math.Round(consumed),
                caloriesBurned = Math.Round(burned),
                netCalories = Math.Round(net),
                deltaFromMaintenance = Math.Round(delta),
                status = delta < 0 ? "Deficit" : delta > 0 ? "Surplus" : "Maintenance"
            });
        }

        // ---------------- TIMELINE (CHARTS) ----------------
        [HttpGet("timeline/{userId}")]
        public async Task<IActionResult> GetTimeline(string userId, int days = 30)
        {
            var user = await _userService.GetByClientIdAsync(userId);
            if (user == null) return NotFound();

            double heightCm = 170;
            TryGetDouble(user, "HeightCm", ref heightCm);
            TryGetDouble(user, "Height", ref heightCm);
            TryGetDouble(user, "height", ref heightCm);

            int age = 25;
            TryGetInt(user, "Age", ref age);
            TryGetInt(user, "age", ref age);

            string gender = "Male";
            TryGetString(user, "Gender", ref gender);
            TryGetString(user, "gender", ref gender);
            bool isFemale = gender.Equals("female", StringComparison.OrdinalIgnoreCase);

            double activityFactor = 1.55;
            string activityLevel = "";
            if (TryGetString(user, "ActivityLevel", ref activityLevel) ||
                TryGetString(user, "activityLevel", ref activityLevel))
            {
                activityFactor = activityLevel switch
                {
                    "Sedentary" => 1.2,
                    "Light" => 1.375,
                    "Moderate" => 1.55,
                    "Active" => 1.725,
                    "VeryActive" => 1.9,
                    _ => 1.55
                };
            }

            var weights = await _weightService.GetLastNDaysAsync(userId, days);

            var timeline = weights.Select(w =>
            {
                double bmr = isFemale
                    ? (10 * w.WeightKg) + (6.25 * heightCm) - (5 * age) - 161
                    : (10 * w.WeightKg) + (6.25 * heightCm) - (5 * age) + 5;

                return new
                {
                    date = w.LoggedAt.Date,
                    weight = w.WeightKg,
                    bmr = Math.Round(bmr),
                    maintenance = Math.Round(bmr * activityFactor)
                };
            });

            return Ok(timeline);
        }

        // ----------------- reflection-safe helpers -----------------

        private static bool TryGetDouble(object obj, string propName, ref double target)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return false;

                var val = p.GetValue(obj);
                if (val == null) return false;

                if (val is double d && d > 0) { target = d; return true; }
                if (val is float f && f > 0) { target = f; return true; }
                if (val is int i && i > 0) { target = i; return true; }
                if (val is long l && l > 0) { target = l; return true; }

                if (val is string s)
                {
                    s = s.Trim();
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
                    {
                        target = parsed;
                        return true;
                    }
                    // try current culture too
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed) && parsed > 0)
                    {
                        target = parsed;
                        return true;
                    }
                }

                return false;
            }
            catch { return false; }
        }

        private static bool TryGetInt(object obj, string propName, ref int target)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return false;

                var val = p.GetValue(obj);
                if (val == null) return false;

                if (val is int i && i > 0) { target = i; return true; }
                if (val is long l && l > 0) { target = (int)l; return true; }

                if (val is string s)
                {
                    s = s.Trim();
                    if (int.TryParse(s, out var parsed) && parsed > 0)
                    {
                        target = parsed;
                        return true;
                    }
                }

                return false;
            }
            catch { return false; }
        }

        private static bool TryGetString(object obj, string propName, ref string target)
        {
            try
            {
                var p = obj.GetType().GetProperty(propName);
                if (p == null) return false;

                var val = p.GetValue(obj)?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    target = val.Trim();
                    return true;
                }

                return false;
            }
            catch { return false; }
        }
    }
}

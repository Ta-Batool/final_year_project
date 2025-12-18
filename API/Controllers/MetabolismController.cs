using Microsoft.AspNetCore.Mvc;
using API.Services;
using Model;

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

            double weightKg = latestWeight.WeightKg;
            double heightCm = double.Parse(user.Height); // stored as string in your project
            int age = DateTime.UtcNow.Year - user.DateOfBirth.Year;
            bool isMale = user.Gender.ToLower() == "male";

            double bmr = isMale
                ? (10 * weightKg) + (6.25 * heightCm) - (5 * age) + 5
                : (10 * weightKg) + (6.25 * heightCm) - (5 * age) - 161;

            double activityFactor = user.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Light" => 1.375,
                "Moderate" => 1.55,
                "Active" => 1.725,
                "VeryActive" => 1.9,
                _ => 1.55
            };

            double maintenance = bmr * activityFactor;

            var today = DateTime.UtcNow.Date;

            double consumed = await _mealService.GetCaloriesForDateAsync(userId, today);
            double burned = await _exerciseService.GetCaloriesBurnedForDateAsync(userId, today);

            double net = consumed - burned;
            double delta = net - maintenance;

            return Ok(new
            {
                weightKg,
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

            double heightCm = double.Parse(user.Height);
            int age = DateTime.UtcNow.Year - user.DateOfBirth.Year;
            bool isMale = user.Gender.ToLower() == "male";

            double activityFactor = user.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Light" => 1.375,
                "Moderate" => 1.55,
                "Active" => 1.725,
                "VeryActive" => 1.9,
                _ => 1.55
            };

            var weights = await _weightService.GetLastNDaysAsync(userId, days);

            var timeline = weights.Select(w =>
            {
                double bmr = isMale
                    ? (10 * w.WeightKg) + (6.25 * heightCm) - (5 * age) + 5
                    : (10 * w.WeightKg) + (6.25 * heightCm) - (5 * age) - 161;

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
    }
}

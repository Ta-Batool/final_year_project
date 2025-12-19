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

            double weightKg = 70;
            double heightCm = 170;
            int age = 25;
            string gender = "Male";

            if (user != null)
            {
                if (user.WeightKg > 0) weightKg = user.WeightKg;
                if (user.HeightCm > 0) heightCm = user.HeightCm;
                if (user.Age > 0) age = user.Age;
                if (!string.IsNullOrWhiteSpace(user.Gender))
                    gender = user.Gender;
            }

            var meals = await _mealService.GetTodayMealsAsync(clientId) ?? new();
            var exercises = await _exerciseService.GetTodayAsync(clientId) ?? new();

            int consumed = meals.Sum(m => m.Calories ?? 0);
            int burned = exercises.Sum(e => e.CaloriesBurned ?? 0);

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
    }
}

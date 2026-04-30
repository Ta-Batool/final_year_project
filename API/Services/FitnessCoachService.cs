using API.Ai;
using Model;
using MongoDB.Driver;
using System.Globalization;
using System.Text.Json;

namespace API.Services
{
    public class FitnessCoachService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<HealthLog> _healthLogs;
        private readonly IMongoCollection<WeightLog> _weightLogs;
        private readonly IMongoCollection<Meal> _meals;
        private readonly IMongoCollection<ExerciseEntry> _exercises;
        private readonly IMongoCollection<DailyCheckIn> _checkIns;
        private readonly IAiAssistantService _ai;

        public FitnessCoachService(IMongoDatabase database, IAiAssistantService ai)
        {
            _users = database.GetCollection<User>("User");
            _healthLogs = database.GetCollection<HealthLog>("HealthLogs");
            _weightLogs = database.GetCollection<WeightLog>("WeightLog");
            _meals = database.GetCollection<Meal>("Meals");
            _exercises = database.GetCollection<ExerciseEntry>("ExerciseEntries");
            _checkIns = database.GetCollection<DailyCheckIn>("DailyCheckIns");
            _ai = ai;
        }

        public async Task<object> GetMonthlySummaryAsync(string clientId, int year, int month)
        {
            var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1);

            var user = await _users.Find(x => x.ClientId == clientId).FirstOrDefaultAsync();

            var healthLogs = await _healthLogs.Find(x =>
                    x.UserId == clientId &&
                    x.Timestamp >= from &&
                    x.Timestamp < to)
                .SortBy(x => x.Timestamp)
                .ToListAsync();

            var weightLogs = await _weightLogs.Find(x =>
                    x.UserId == clientId &&
                    x.LoggedAt >= from &&
                    x.LoggedAt < to)
                .SortBy(x => x.LoggedAt)
                .ToListAsync();

            var meals = await _meals.Find(x =>
                    x.ClientId == clientId &&
                    x.Date >= from &&
                    x.Date < to)
                .SortBy(x => x.Date)
                .ToListAsync();

            var exercises = await _exercises.Find(x =>
                    x.ClientId == clientId &&
                    x.Date >= from &&
                    x.Date < to)
                .SortBy(x => x.Date)
                .ToListAsync();

            var checkIns = await _checkIns.Find(x =>
                    x.ClientId == clientId &&
                    x.DateUtc >= from &&
                    x.DateUtc < to)
                .SortBy(x => x.DateUtc)
                .ToListAsync();

            double profileWeight = ParseDouble(user?.Weight);
            double profileHeight = ParseHeightCm(user?.Height);
            int age = user?.Age > 0 ? user.Age : 25;
            string gender = !string.IsNullOrWhiteSpace(user?.Gender) ? user.Gender! : user?.Sex ?? "Male";

            double latestWeight =
                weightLogs.LastOrDefault()?.WeightKg > 0 ? weightLogs.Last().WeightKg :
                healthLogs.LastOrDefault(x => x.WeightKg > 0)?.WeightKg > 0 ? healthLogs.Last(x => x.WeightKg > 0).WeightKg :
                checkIns.LastOrDefault(x => x.WeightKg > 0)?.WeightKg > 0 ? checkIns.Last(x => x.WeightKg > 0).WeightKg :
                profileWeight;

            double latestHeight =
                healthLogs.LastOrDefault(x => x.HeightCm > 0)?.HeightCm > 0 ? healthLogs.Last(x => x.HeightCm > 0).HeightCm :
                checkIns.LastOrDefault(x => x.HeightCm > 0)?.HeightCm > 0 ? checkIns.Last(x => x.HeightCm > 0).HeightCm :
                profileHeight;

            var firstWeight =
                weightLogs.FirstOrDefault()?.WeightKg > 0 ? weightLogs.First().WeightKg :
                healthLogs.FirstOrDefault(x => x.WeightKg > 0)?.WeightKg > 0 ? healthLogs.First(x => x.WeightKg > 0).WeightKg :
                checkIns.FirstOrDefault(x => x.WeightKg > 0)?.WeightKg > 0 ? checkIns.First(x => x.WeightKg > 0).WeightKg :
                latestWeight;

            double? bmi = null;
            if (latestWeight > 0 && latestHeight > 0)
            {
                var h = latestHeight / 100.0;
                bmi = Math.Round(latestWeight / (h * h), 2);
            }

            int bmr = 0;
            int maintenance = 0;

            if (latestWeight > 0 && latestHeight > 0)
            {
                var female = gender.Equals("Female", StringComparison.OrdinalIgnoreCase);
                var rawBmr = female
                    ? (10 * latestWeight) + (6.25 * latestHeight) - (5 * age) - 161
                    : (10 * latestWeight) + (6.25 * latestHeight) - (5 * age) + 5;

                bmr = (int)Math.Round(rawBmr);
                maintenance = (int)Math.Round(rawBmr * 1.375); // light activity default
            }

            int totalCaloriesConsumed = meals.Sum(x => x.Calories ?? 0);
            int totalCaloriesBurned = exercises.Sum(x => x.CaloriesBurned ?? 0);
            int mealDays = meals.Select(x => x.Date.Date).Distinct().Count();
            int exerciseDays = exercises.Select(x => x.Date.Date).Distinct().Count();

            double avgCaloriesConsumed = mealDays > 0
                ? Math.Round((double)totalCaloriesConsumed / mealDays)
                : 0;

            double avgCaloriesBurned = exerciseDays > 0
                ? Math.Round((double)totalCaloriesBurned / exerciseDays)
                : 0;

            double avgSteps = checkIns.Any(x => x.Steps > 0)
                ? Math.Round(checkIns.Where(x => x.Steps > 0).Average(x => x.Steps))
                : 0;

            double avgExerciseMinutes =
                exercises.Any(x => x.DurationMinutes > 0)
                    ? Math.Round(exercises.Where(x => x.DurationMinutes > 0).Average(x => x.DurationMinutes!.Value))
                    : checkIns.Any(x => x.ExerciseMinutes > 0)
                        ? Math.Round(checkIns.Where(x => x.ExerciseMinutes > 0).Average(x => x.ExerciseMinutes))
                        : 0;

            int dailyAverageNet = (int)Math.Round(avgCaloriesConsumed - avgCaloriesBurned);
            int dailyDeficitOrSurplus = maintenance > 0 ? dailyAverageNet - maintenance : 0;

            double expectedMonthlyWeightChangeKg = maintenance > 0
                ? Math.Round((dailyDeficitOrSurplus * DateTime.DaysInMonth(year, month)) / 7700.0, 2)
                : 0;

            return new
            {
                year,
                month,
                hasData = user != null || healthLogs.Any() || weightLogs.Any() || meals.Any() || exercises.Any() || checkIns.Any(),

                profile = new
                {
                    name = user?.Name,
                    gender,
                    age,
                    profileWeight,
                    profileHeight
                },

                checkins = checkIns.Count,
                healthLogs = healthLogs.Count,
                weightLogs = weightLogs.Count,

                latestWeightKg = latestWeight,
                latestHeightCm = latestHeight,
                startWeightKg = firstWeight,
                endWeightKg = latestWeight,
                changeKg = latestWeight > 0 && firstWeight > 0 ? Math.Round(latestWeight - firstWeight, 2) : 0,
                bmi,

                bmr,
                maintenanceCalories = maintenance,

                mealsLogged = meals.Count,
                mealDays,
                totalCaloriesConsumed,
                avgCaloriesConsumed,

                exercisesLogged = exercises.Count,
                exerciseDays,
                totalCaloriesBurned,
                avgCaloriesBurned,
                avgSteps,
                avgExerciseMinutes,

                dailyAverageNetCalories = dailyAverageNet,
                dailyDeficitOrSurplus,
                expectedMonthlyWeightChangeKg,

                recentMeals = meals
                    .OrderByDescending(x => x.Date)
                    .Take(10)
                    .Select(x => new
                    {
                        date = x.Date.ToString("yyyy-MM-dd"),
                        x.Type,
                        x.Foods,
                        x.Calories
                    }),

                recentExercises = exercises
                    .OrderByDescending(x => x.Date)
                    .Take(10)
                    .Select(x => new
                    {
                        date = x.Date.ToString("yyyy-MM-dd"),
                        x.Name,
                        x.Type,
                        x.DurationMinutes,
                        x.CaloriesBurned
                    }),

                message = "Summary built from profile, health logs, weight logs, meals, exercises and check-ins."
            };
        }

        public async Task<object> ChatAsync(string clientId, string message, int year, int month)
        {
            var summary = await GetMonthlySummaryAsync(clientId, year, month);

            var contextJson = JsonSerializer.Serialize(summary, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var prompt = $"""
            You are NutriNest AI Fitness Coach.

            Use this real user data:
            {contextJson}

            User question:
            {message}

            Your job:
            - Answer the exact question.
            - Use profile weight, height, BMI, BMR, maintenance calories, food calories, exercise calories burned, net calories and health logs.
            - Compare daily calorie intake with calories burned and BMR/maintenance.
            - Predict whether the user is likely losing, gaining or maintaining weight.
            - Give personalized suggestions based on missing or available data.
            - If some data is missing, clearly say what is missing.
            - Do not give generic repeated answers.
            - Keep it practical and friendly.
            - Do not diagnose disease or prescribe medicine.
            """;

            var reply = await _ai.GetPatientReplyAsync(clientId, prompt);

            return new
            {
                clientId,
                year,
                month,
                reply,
                context = summary
            };
        }

        private static double ParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            value = value.Trim().Replace("kg", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("cm", "", StringComparison.OrdinalIgnoreCase);

            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0;
        }

        private static double ParseHeightCm(string? value)
        {
            var h = ParseDouble(value);
            if (h <= 0) return 0;

            // If user entered 5.7, treat it as feet-style height and convert approx to cm
            if (h > 3 && h < 9)
                return Math.Round(h * 30.48, 2);

            return h;
        }
    }
}
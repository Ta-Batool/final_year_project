using Model;

namespace API.Services
{
    public class FitnessCoachService
    {
        private readonly CheckInService _checkIns;

        public FitnessCoachService(CheckInService checkIns)
        {
            _checkIns = checkIns;
        }

        public static double CalcBmi(double weightKg, double heightCm)
        {
            if (heightCm <= 0) return 0;
            var h = heightCm / 100.0;
            return weightKg / (h * h);
        }

        public async Task<object> GetMonthlySummaryAsync(string clientId, int year, int month)
        {
            var items = await _checkIns.GetMonthAsync(clientId, year, month);

            if (items == null || items.Count == 0)
            {
                return new
                {
                    year,
                    month,
                    hasData = false,
                    message = "No check-ins found for this month."
                };
            }

            var ordered = items.OrderBy(x => x.DateUtc).ToList();

            var first = ordered.First();
            var last = ordered.Last();

            double startW = first.WeightKg;
            double endW = last.WeightKg;
            double diff = endW - startW;

            double avgSteps = ordered.Average(x => x.Steps);
            double avgExercise = ordered.Average(x => x.ExerciseMinutes);

            double bmiStart = CalcBmi(startW, first.HeightCm);
            double bmiEnd = CalcBmi(endW, last.HeightCm);

            double expectedLossKg =
                (avgExercise >= 30 && avgSteps >= 6000) ? 1.5 :
                (avgExercise >= 20 || avgSteps >= 5000) ? 1.0 :
                0.5;

            var suggestions = new List<string>
            {
                "Aim for protein in each meal (eggs, chicken, dal) to reduce cravings.",
                "Try 7–8 hours of sleep; poor sleep increases hunger.",
                "Keep water intake around 8–10 cups daily."
            };

            if (avgExercise < 20)
                suggestions.Add("Increase exercise to at least 20–30 minutes per day, such as a brisk walk.");

            if (avgSteps < 5000)
                suggestions.Add("Gradually target 6,000 to 8,000 steps per day.");

            var routine = new List<object>
            {
                new { day = "Mon", plan = "30 min brisk walk + 10 min stretching" },
                new { day = "Tue", plan = "Bodyweight: squats 3x12, pushups 3x8, plank 3x30s" },
                new { day = "Wed", plan = "35 min brisk walk" },
                new { day = "Thu", plan = "Lower body: lunges 3x10, glute bridge 3x12, calf raises 3x15" },
                new { day = "Fri", plan = "40 min walk + mobility" },
                new { day = "Sat", plan = "Full body: squats 3x12, rows/band 3x12, plank 3x40s" },
                new { day = "Sun", plan = "Light walk 20 min + rest" }
            };

            return new
            {
                year,
                month,
                hasData = true,
                checkins = ordered.Count,
                startWeightKg = Math.Round(startW, 2),
                endWeightKg = Math.Round(endW, 2),
                changeKg = Math.Round(diff, 2),
                bmiStart = Math.Round(bmiStart, 2),
                bmiEnd = Math.Round(bmiEnd, 2),
                avgSteps = Math.Round(avgSteps, 0),
                avgExerciseMinutes = Math.Round(avgExercise, 0),
                expectedNextMonthLossKg = expectedLossKg,
                suggestions,
                routine
            };
        }

        public async Task<object> ChatAsync(string clientId, string message, int year, int month)
        {
            var summary = await GetMonthlySummaryAsync(clientId, year, month);
            var msg = (message ?? "").ToLowerInvariant();

            string reply =
                msg.Contains("diet") || msg.Contains("food")
                    ? "Share your last 2 meals plus snacks, and I’ll suggest calorie-friendly swaps."
                : msg.Contains("workout") || msg.Contains("exercise")
                    ? "Tell me your available time per day (20/30/45 min) and equipment (none/dumbbells), and I’ll adjust your plan."
                : msg.Contains("bmi")
                    ? "BMI is only a rough indicator. Weight trend, waist measurement, sleep, and activity matter too."
                : "I’m your fitness coach. Send your food and exercise routine today, and I’ll guide you. You can also ask any health or fitness question.";

            return new
            {
                clientId,
                year,
                month,
                reply,
                context = summary
            };
        }
    }
}
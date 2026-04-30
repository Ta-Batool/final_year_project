using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class CheckInService
    {
        private readonly IMongoCollection<DailyCheckIn> _checkIns;

        public CheckInService(IMongoDatabase db)
        {
            _checkIns = db.GetCollection<DailyCheckIn>("DailyCheckIns");
        }

        // One check-in per client per day
        public async Task<DailyCheckIn> UpsertAsync(DailyCheckIn input)
        {
            input.DateUtc = input.DateUtc.Date;

            var filter = Builders<DailyCheckIn>.Filter.Where(x =>
                x.ClientId == input.ClientId && x.DateUtc == input.DateUtc);

            var existing = await _checkIns.Find(filter).FirstOrDefaultAsync();

            if (existing is null)
            {
                await _checkIns.InsertOneAsync(input);
                return input;
            }

            // Safe merge: only overwrite when meaningful value is provided
            existing.WeightKg = input.WeightKg > 0 ? input.WeightKg : existing.WeightKg;
            existing.HeightCm = input.HeightCm > 0 ? input.HeightCm : existing.HeightCm;
            existing.Steps = input.Steps > 0 ? input.Steps : existing.Steps;
            existing.ExerciseMinutes = input.ExerciseMinutes > 0 ? input.ExerciseMinutes : existing.ExerciseMinutes;
            existing.SleepHours = input.SleepHours > 0 ? input.SleepHours : existing.SleepHours;
            existing.WaterCups = input.WaterCups > 0 ? input.WaterCups : existing.WaterCups;

            if (!string.IsNullOrWhiteSpace(input.FoodNotes))
                existing.FoodNotes = input.FoodNotes.Trim();

            if (!string.IsNullOrWhiteSpace(input.ExerciseNotes))
                existing.ExerciseNotes = input.ExerciseNotes.Trim();

            await _checkIns.ReplaceOneAsync(filter, existing);
            return existing;
        }

        public async Task<List<DailyCheckIn>> GetMonthAsync(string clientId, int year, int month)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var filter = Builders<DailyCheckIn>.Filter.Where(x =>
                x.ClientId == clientId &&
                x.DateUtc >= start &&
                x.DateUtc < end);

            return await _checkIns
                .Find(filter)
                .SortBy(x => x.DateUtc)
                .ToListAsync();
        }
    }
}
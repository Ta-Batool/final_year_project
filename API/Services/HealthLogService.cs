using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class HealthLogService
    {
        private readonly IMongoCollection<HealthLog> _logs;

        public HealthLogService(IOptions<MongoDBSettings> mongoOptions)
        {
            var s = mongoOptions.Value;
            var client = new MongoClient(s.ConnectionString);
            var db = client.GetDatabase(s.DatabaseName);
            _logs = db.GetCollection<HealthLog>("HealthLogs");
        }

        public Task AddAsync(HealthLog log) => _logs.InsertOneAsync(log);

        public Task<List<HealthLog>> GetRangeAsync(string userId, DateTime from, DateTime to) =>
            _logs.Find(x => x.UserId == userId && x.Timestamp >= from && x.Timestamp < to)
                 .SortBy(x => x.Timestamp)
                 .ToListAsync();

        public async Task<object> GetSummaryAsync(string userId, DateTime from, DateTime to)
        {
            var data = await GetRangeAsync(userId, from, to);
            if (!data.Any()) return new { count = 0 };

            double avgSys = data.Average(x => x.Systolic);
            double avgDia = data.Average(x => x.Diastolic);
            double avgGlu = data.Average(x => x.Glucose);
            double lastW = data.Last().WeightKg;
            double lastH = data.Last().HeightCm;

            double bmi = lastH > 0 ? lastW / Math.Pow(lastH / 100.0, 2) : 0;

            return new {
                count = data.Count,
                from, to,
                avgSystolic = avgSys,
                avgDiastolic = avgDia,
                avgGlucose = avgGlu,
                lastWeightKg = lastW,
                lastHeightCm = lastH,
                bmi
            };
        }
    }
}

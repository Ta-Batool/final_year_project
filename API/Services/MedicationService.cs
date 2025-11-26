using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IMongoCollection<MedicationPlan> _plans;
        private readonly IMongoCollection<MedicationLog> _logs;

        public MedicationService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDBSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDBSettings:DatabaseName"]);

            _plans = database.GetCollection<MedicationPlan>("MedicationPlans");
            _logs = database.GetCollection<MedicationLog>("MedicationLogs");
        }

        public async Task<List<MedicationPlan>> GetPlansAsync(string userId)
        {
            return await _plans.Find(p => p.UserId == userId).ToListAsync();
        }

        public async Task AddPlanAsync(MedicationPlan plan)
        {
            plan.Id = null;
            await _plans.InsertOneAsync(plan);

            // create today's log
            var log = new MedicationLog
            {
                UserId = plan.UserId,
                MedicationPlanId = plan.Id!,
                ScheduledAt = DateTime.UtcNow.Date.Add(TimeSpan.Parse(plan.TimeOfDay)),
                Status = MedicationStatus.Upcoming
            };

            await _logs.InsertOneAsync(log);
        }

        public async Task DeletePlanAsync(string id)
        {
            await _plans.DeleteOneAsync(p => p.Id == id);
            await _logs.DeleteManyAsync(l => l.MedicationPlanId == id);
        }

        public async Task<List<MedicationLog>> GetTodayLogsAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _logs.Find(l =>
                l.UserId == userId &&
                l.ScheduledAt >= today &&
                l.ScheduledAt < tomorrow
            ).ToListAsync();
        }

        public async Task UpdateLogStatusAsync(string logId, MedicationStatus status)
        {
            var update = Builders<MedicationLog>.Update
                .Set(l => l.Status, status);

            await _logs.UpdateOneAsync(
                l => l.Id == logId,
                update
            );
        }
    }
}

using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IMongoCollection<MedicationPlan> _plans;
        private readonly IMongoCollection<MedicationLog> _logs;

        public MedicationService(IOptions<MongoDBSettings> dbOptions)
        {
            var settings = dbOptions.Value ?? throw new ArgumentNullException(nameof(dbOptions));

            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _plans = database.GetCollection<MedicationPlan>("MedicationPlans");
            _logs  = database.GetCollection<MedicationLog>("MedicationLogs");
        }

        public async Task<List<MedicationPlan>> GetPlansAsync(string userId)
        {
            var filter = Builders<MedicationPlan>.Filter.Eq(p => p.UserId, userId);
            return await _plans.Find(filter).ToListAsync();
        }

        public async Task AddPlanAsync(MedicationPlan plan)
        {
            if (string.IsNullOrWhiteSpace(plan.UserId))
                throw new ArgumentException("UserId is required for MedicationPlan.");

            plan.Id ??= ObjectId.GenerateNewId().ToString();
            plan.StartDate = plan.StartDate.Date;

            await _plans.InsertOneAsync(plan);

            // Also create log for today as "Upcoming"
            var todayLog = new MedicationLog
            {
                PlanId = plan.Id,
                UserId = plan.UserId,
                Date = DateTime.UtcNow.Date,
                Status = MedicationStatus.Upcoming
            };

            await _logs.InsertOneAsync(todayLog);
        }

        public async Task DeletePlanAsync(string id)
        {
            var filter = Builders<MedicationPlan>.Filter.Eq(p => p.Id, id);
            await _plans.DeleteOneAsync(filter);

            var logsFilter = Builders<MedicationLog>.Filter.Eq(l => l.PlanId, id);
            await _logs.DeleteManyAsync(logsFilter);
        }

        public async Task<List<MedicationLog>> GetTodayLogsAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            var filter = Builders<MedicationLog>.Filter.And(
                Builders<MedicationLog>.Filter.Eq(l => l.UserId, userId),
                Builders<MedicationLog>.Filter.Gte(l => l.Date, today),
                Builders<MedicationLog>.Filter.Lt(l => l.Date, today.AddDays(1))
            );

            return await _logs.Find(filter).ToListAsync();
        }

        public async Task UpdateLogStatusAsync(string logId, MedicationStatus status)
        {
            var filter = Builders<MedicationLog>.Filter.Eq(l => l.Id, logId);
            var update = Builders<MedicationLog>.Update.Set(l => l.Status, status);

            await _logs.UpdateOneAsync(filter, update);
        }
    }
}

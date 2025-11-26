using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Services
{
    public interface IMedicationService
    {
        Task<List<MedicationPlan>> GetPlansAsync(string userId);
        Task<MedicationPlan> AddPlanAsync(MedicationPlan plan);
        Task DeletePlanAsync(string id);
        Task<List<MedicationLog>> GetTodayLogsAsync(string userId);
        Task UpdateLogStatusAsync(string logId, MedicationStatus status);
    }

    public class MedicationService : IMedicationService
    {
        private readonly IMongoCollection<MedicationPlan> _plans;
        private readonly IMongoCollection<MedicationLog> _logs;

        public MedicationService(IMongoClient client, IOptions<MongoDbSettings> options)
        {
            var db = client.GetDatabase(options.Value.DatabaseName);

            _plans = db.GetCollection<MedicationPlan>("MedicationPlans");
            _logs  = db.GetCollection<MedicationLog>("MedicationLogs");
        }

        public async Task<List<MedicationPlan>> GetPlansAsync(string userId)
        {
            return await _plans
                .Find(p => p.UserId == userId)
                .SortBy(p => p.TimeOfDay)
                .ToListAsync();
        }

        public async Task<MedicationPlan> AddPlanAsync(MedicationPlan plan)
        {
            // Ensure Id is null so Mongo generates a new ObjectId
            plan.Id = null;

            await _plans.InsertOneAsync(plan);

            // When you create a plan you can also pre-create “today” log if you like.
            // For now we’ll keep logs separate and generated when needed.

            return plan;
        }

        public async Task DeletePlanAsync(string id)
        {
            if (!ObjectId.TryParse(id, out _)) return;

            await _plans.DeleteOneAsync(p => p.Id == id);
            await _logs.DeleteManyAsync(l => l.PlanId == id);
        }

        public async Task<List<MedicationLog>> GetTodayLogsAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            var filter = Builders<MedicationLog>.Filter.And(
                Builders<MedicationLog>.Filter.Eq(l => l.UserId, userId),
                Builders<MedicationLog>.Filter.Gte(l => l.Date, today),
                Builders<MedicationLog>.Filter.Lt(l => l.Date, today.AddDays(1))
            );

            return await _logs.Find(filter)
                              .SortBy(l => l.ScheduledTime)
                              .ToListAsync();
        }

        public async Task UpdateLogStatusAsync(string logId, MedicationStatus status)
        {
            if (!ObjectId.TryParse(logId, out _)) return;

            var update = Builders<MedicationLog>.Update
                .Set(l => l.Status, status)
                .Set(l => l.UpdatedAt, DateTime.UtcNow);

            await _logs.UpdateOneAsync(l => l.Id == logId, update);
        }
    }
}

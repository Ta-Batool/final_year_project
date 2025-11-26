using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IMongoCollection<MedicationPlan> _plans;
        private readonly IMongoCollection<MedicationLog> _logs;

        public MedicationService(IOptions<MongoDBSettings> mongoOptions)
        {
            var settings = mongoOptions.Value;

            var client = new MongoClient(settings.ConnectionString);
            var db = client.GetDatabase(settings.DatabaseName);

            // You can change collection names if you like
            _plans = db.GetCollection<MedicationPlan>("MedicationPlans");
            _logs  = db.GetCollection<MedicationLog>("MedicationLogs");
        }

        public async Task<List<MedicationPlan>> GetPlansAsync(string userId)
        {
            return await _plans.Find(p => p.UserId == userId).ToListAsync();
        }

        public async Task AddPlanAsync(MedicationPlan plan)
        {
            if (string.IsNullOrWhiteSpace(plan.UserId))
                throw new ArgumentException("UserId is required", nameof(plan));

            plan.StartDate = plan.StartDate.Date;
            if (plan.EndDate.HasValue)
                plan.EndDate = plan.EndDate.Value.Date;

            await _plans.InsertOneAsync(plan);

            var today = DateTime.UtcNow.Date;
            if (today >= plan.StartDate && (!plan.EndDate.HasValue || today <= plan.EndDate.Value))
            {
                var log = new MedicationLog
                {
                    PlanId = plan.Id!,
                    UserId = plan.UserId,
                    Date = today,
                    Status = MedicationStatus.Pending
                };

                await _logs.InsertOneAsync(log);
            }
        }

        public async Task DeletePlanAsync(string id)
        {
            await _plans.DeleteOneAsync(p => p.Id == id);
            await _logs.DeleteManyAsync(l => l.PlanId == id);
        }

        public async Task<List<MedicationLog>> GetTodayLogsAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;

            var filterUser = Builders<MedicationLog>.Filter.Eq(l => l.UserId, userId);
            var filterDate = Builders<MedicationLog>.Filter.Eq(l => l.Date, today);

            var logs = await _logs.Find(filterUser & filterDate).ToListAsync();

            // If no logs exist for today, create them for all active plans
            if (logs.Count == 0)
            {
                var plans = await _plans.Find(p =>
                    p.UserId == userId &&
                    p.StartDate <= today &&
                    (p.EndDate == null || p.EndDate >= today)
                ).ToListAsync();

                if (plans.Any())
                {
                    var newLogs = plans.Select(p => new MedicationLog
                    {
                        PlanId = p.Id!,
                        UserId = p.UserId,
                        Date = today,
                        Status = MedicationStatus.Pending
                    }).ToList();

                    await _logs.InsertManyAsync(newLogs);
                    logs = newLogs;
                }
            }

            return logs;
        }

        public async Task UpdateLogStatusAsync(string logId, MedicationStatus status)
        {
            var update = Builders<MedicationLog>.Update.Set(l => l.Status, status);
            await _logs.UpdateOneAsync(l => l.Id == logId, update);
        }
    }
}

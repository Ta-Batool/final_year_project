using MongoDB.Driver;
using Microsoft.Extensions.Options;
using API.MongoModel;
using Model;

namespace API.Services
{
    public class WeightLogService
    {
        private readonly IMongoCollection<WeightLog> _collection;

        public WeightLogService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            _collection = database.GetCollection<WeightLog>("WeightLog");
        }

        public async Task<List<WeightLog>> GetByUserAsync(string userId)
        {
            return await _collection
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.LoggedAt)
                .ToListAsync();
        }

        // ✅ REQUIRED BY MetabolismController
        public async Task<WeightLog?> GetLatestWeightAsync(string userId)
        {
            return await _collection
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.LoggedAt)
                .FirstOrDefaultAsync();
        }

        // ✅ REQUIRED BY MetabolismController timeline
        public async Task<List<WeightLog>> GetLastNDaysAsync(string userId, int days)
        {
            if (days <= 0) days = 30;

            var start = DateTime.UtcNow.Date.AddDays(-days + 1);

            // Only weights within last N days
            return await _collection
                .Find(x => x.UserId == userId && x.LoggedAt >= start)
                .SortBy(x => x.LoggedAt)
                .ToListAsync();
        }

        public async Task<WeightLog> CreateAsync(WeightLog log)
        {
            await _collection.InsertOneAsync(log);
            return log;
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}

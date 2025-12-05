using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class HydrationService : IHydrationService
    {
        private readonly IMongoCollection<HydrationLog> _collection;

        public HydrationService(IOptions<MongoDBSettings> mongoOptions)
        {
            var settings = mongoOptions.Value;
            var client = new MongoClient(settings.ConnectionString);
            var db = client.GetDatabase(settings.DatabaseName);

            _collection = db.GetCollection<HydrationLog>("HydrationLogs");
        }

        public async Task<List<HydrationLog>> GetForDayAsync(string clientId, DateTime dateUtc)
        {
            var day = dateUtc.Date;

            var filter = Builders<HydrationLog>.Filter.And(
                Builders<HydrationLog>.Filter.Eq(x => x.ClientId, clientId),
                Builders<HydrationLog>.Filter.Eq(x => x.Date, day)
            );

            return await _collection.Find(filter)
                .SortBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<HydrationLog> AddAsync(HydrationLog log)
        {
            // normalize
            log.Date = log.Date == default ? DateTime.UtcNow.Date : log.Date.Date;
            log.CreatedAt = DateTime.UtcNow;

            await _collection.InsertOneAsync(log);
            return log;
        }

        public async Task AddWaterAsync(string clientId, int amountMl)
        {
            var today = DateTime.UtcNow.Date;

            var filter = Builders<HydrationLog>.Filter.And(
                Builders<HydrationLog>.Filter.Eq(x => x.ClientId, clientId),
                Builders<HydrationLog>.Filter.Eq(x => x.Date, today)
            );

            // if document for today does not exist, create it with default target 2000ml
            var update = Builders<HydrationLog>.Update
                .Inc(x => x.TotalMl, amountMl)
                .SetOnInsert(x => x.ClientId, clientId)
                .SetOnInsert(x => x.Date, today)
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow)
                .SetOnInsert(x => x.TargetMl, 2000);

            var options = new UpdateOptions { IsUpsert = true };

            await _collection.UpdateOneAsync(filter, update, options);
        }

        public async Task UpdateTargetAsync(string clientId, int targetMl)
        {
            var today = DateTime.UtcNow.Date;

            var filter = Builders<HydrationLog>.Filter.And(
                Builders<HydrationLog>.Filter.Eq(x => x.ClientId, clientId),
                Builders<HydrationLog>.Filter.Eq(x => x.Date, today)
            );

            var update = Builders<HydrationLog>.Update
                .Set(x => x.TargetMl, targetMl)
                .SetOnInsert(x => x.TotalMl, 0)
                .SetOnInsert(x => x.ClientId, clientId)
                .SetOnInsert(x => x.Date, today)
                .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow);

            var options = new UpdateOptions { IsUpsert = true };

            await _collection.UpdateOneAsync(filter, update, options);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}

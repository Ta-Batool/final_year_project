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
            var start = dateUtc.Date;
            var end = start.AddDays(1);

            var filter = Builders<HydrationLog>.Filter.And(
                Builders<HydrationLog>.Filter.Eq(x => x.ClientId, clientId),
                Builders<HydrationLog>.Filter.Gte(x => x.Date, start),
                Builders<HydrationLog>.Filter.Lt(x => x.Date, end)
            );

            return await _collection.Find(filter)
                .SortBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<HydrationLog> AddAsync(HydrationLog log)
        {
            log.Date = log.Date.Date.ToUniversalTime();
            log.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(log);
            return log;
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}

using MongoDB.Driver;
using Microsoft.Extensions.Options;
using API.MongoModel;
using Model;

namespace API.Services
{
    public class GlucoseLogService
    {
        private readonly IMongoCollection<GlucoseLog> _collection;

        public GlucoseLogService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);

            _collection = database.GetCollection<GlucoseLog>("GlucoseLog");
        }

        public async Task<List<GlucoseLog>> GetByUserAsync(string userId)
        {
            return await _collection
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.LoggedAt)
                .ToListAsync();
        }

        public async Task<GlucoseLog> CreateAsync(GlucoseLog log)
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

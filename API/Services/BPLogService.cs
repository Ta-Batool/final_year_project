using MongoDB.Driver;
using Microsoft.Extensions.Options;
using API.MongoModel;
using Model;

namespace API.Services
{
    public class BPLogService
    {
        private readonly IMongoCollection<BPLog> _col;

        public BPLogService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var db = client.GetDatabase(settings.Value.DatabaseName);
            _col = db.GetCollection<BPLog>("BPLog");
        }

        public Task<List<BPLog>> GetByUserAsync(string userId) =>
            _col.Find(x => x.UserId == userId).SortByDescending(x => x.LoggedAt).ToListAsync();

        public async Task<BPLog> CreateAsync(BPLog log)
        {
            await _col.InsertOneAsync(log);
            return log;
        }

        public Task DeleteAsync(string id) =>
            _col.DeleteOneAsync(x => x.Id == id);
    }
}

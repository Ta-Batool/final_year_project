using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace API.Services
{
    public class DailyChecklistService
    {
        private readonly IMongoCollection<DailyChecklist> _check;

        public DailyChecklistService(IOptions<MongoDBSettings> mongo)
        {
            var client = new MongoClient(mongo.Value.ConnectionString);
            var db = client.GetDatabase(mongo.Value.DatabaseName);
            _check = db.GetCollection<DailyChecklist>("DailyChecklists");
        }

        public Task<DailyChecklist?> GetByUserAndDateAsync(string userId, DateTime date) =>
            _check.Find(x => x.UserId == userId && x.Date == date.Date).FirstOrDefaultAsync();

        public Task CreateAsync(DailyChecklist c) => _check.InsertOneAsync(c);

        public Task UpdateAsync(string id, DailyChecklist c) =>
            _check.ReplaceOneAsync(x => x.Id == id, c);
    }
}

using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace API.Services
{
    public class ExercisePlanService
    {
        private readonly IMongoCollection<ExercisePlan> _ex;

        public ExercisePlanService(IOptions<MongoDBSettings> mongo)
        {
            var client = new MongoClient(mongo.Value.ConnectionString);
            var db = client.GetDatabase(mongo.Value.DatabaseName);
            _ex = db.GetCollection<ExercisePlan>("ExercisePlans");
        }

        public Task<ExercisePlan?> GetByUserAndDateAsync(string userId, DateTime date) =>
            _ex.Find(x => x.UserId == userId && x.Date == date.Date).FirstOrDefaultAsync();

        public Task CreateAsync(ExercisePlan plan) => _ex.InsertOneAsync(plan);

        public Task UpdateAsync(string id, ExercisePlan plan) =>
            _ex.ReplaceOneAsync(x => x.Id == id, plan);
    }
}

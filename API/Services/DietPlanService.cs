using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace API.Services
{
    public class DietPlanService
    {
        private readonly IMongoCollection<DietPlan> _diet;

        public DietPlanService(IOptions<MongoDBSettings> mongo)
        {
            var client = new MongoClient(mongo.Value.ConnectionString);
            var db = client.GetDatabase(mongo.Value.DatabaseName);
            _diet = db.GetCollection<DietPlan>("DietPlans");
        }

        public Task<DietPlan?> GetByUserAndDateAsync(string userId, DateTime date) =>
            _diet.Find(x => x.UserId == userId && x.Date == date.Date).FirstOrDefaultAsync();

        public Task CreateAsync(DietPlan plan) => _diet.InsertOneAsync(plan);

        public Task UpdateAsync(string id, DietPlan plan) =>
            _diet.ReplaceOneAsync(x => x.Id == id, plan);
    }
}

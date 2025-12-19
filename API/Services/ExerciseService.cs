using System;
using System.Linq;
using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IMongoCollection<ExerciseEntry> _collection;

        public ExerciseService(IOptions<MongoDBSettings> mongoOptions)
        {
            var settings = mongoOptions.Value;
            var client = new MongoClient(settings.ConnectionString);
            var db = client.GetDatabase(settings.DatabaseName);

            _collection = db.GetCollection<ExerciseEntry>("ExerciseEntries");
        }

        public async Task<List<ExerciseEntry>> GetForDayAsync(string clientId, DateTime dateUtc)
        {
            var start = dateUtc.Date;
            var end = start.AddDays(1);

            var filter = Builders<ExerciseEntry>.Filter.And(
                Builders<ExerciseEntry>.Filter.Eq(x => x.ClientId, clientId),
                Builders<ExerciseEntry>.Filter.Gte(x => x.Date, start),
                Builders<ExerciseEntry>.Filter.Lt(x => x.Date, end)
            );

            return await _collection.Find(filter)
                .SortBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<ExerciseEntry> AddAsync(ExerciseEntry entry)
        {
            entry.Date = entry.Date.Date.ToUniversalTime();
            entry.CreatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(entry);
            return entry;
        }

        public async Task UpdateStatusAsync(string id, ExerciseStatus status)
        {
            var update = Builders<ExerciseEntry>.Update.Set(x => x.Status, status);
            await _collection.UpdateOneAsync(x => x.Id == id, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }

        // ✅ REQUIRED BY MetabolismController.cs
        public async Task<int> GetCaloriesBurnedForDateAsync(string clientId, DateTime dateUtc)
        {
            var logs = await GetForDayAsync(clientId, dateUtc) ?? new List<ExerciseEntry>();
            return logs.Sum(e => e.CaloriesBurned ?? 0);
        }
    }
}

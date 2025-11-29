using System;
using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class MealService : IMealService
    {
        private readonly IMongoCollection<Meal> _meals;

        public MealService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _meals = database.GetCollection<Meal>("Meal");
        }

        /// <summary>
        /// Core method: get meals for a specific day (UTC window).
        /// Used for "today" and also by-date lookups.
        /// </summary>
        public async Task<List<Meal>> GetMealsForDayAsync(string clientId, DateTime dateUtc)
        {
            var start = dateUtc.Date;
            var end = start.AddDays(1);

            var filter = Builders<Meal>.Filter.And(
                Builders<Meal>.Filter.Eq(m => m.ClientId, clientId),
                Builders<Meal>.Filter.Gte(m => m.Date, start),
                Builders<Meal>.Filter.Lt(m => m.Date, end)
            );

            return await _meals.Find(filter)
                               .SortBy(m => m.CreatedAt)
                               .ToListAsync();
        }

        /// <summary>
        /// Convenience wrapper for "by-date" endpoint.
        /// The date coming from query (yyyy-MM-dd) is usually Unspecified kind,
        /// so we normalize it and then call GetMealsForDayAsync.
        /// </summary>
        public Task<List<Meal>> GetMealsByDateAsync(string clientId, DateTime dateLocal)
        {
            // Normalize to UTC; your Date is stored as date-only anyway.
            var normalized = dateLocal.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateLocal, DateTimeKind.Utc)
                : dateLocal.ToUniversalTime();

            return GetMealsForDayAsync(clientId, normalized);
        }

        public async Task<List<Meal>> GetAllForClientAsync(string clientId)
        {
            return await _meals.Find(m => m.ClientId == clientId)
                               .SortByDescending(m => m.Date)
                               .ToListAsync();
        }

        public async Task<Meal> CreateAsync(Meal meal)
        {
            // Make sure Date and CreatedAt are set so sorting & filtering work
            if (meal.Date == default)
                meal.Date = DateTime.UtcNow.Date;

            if (meal.CreatedAt == default)
                meal.CreatedAt = DateTime.UtcNow;

            await _meals.InsertOneAsync(meal);
            return meal;
        }

        public async Task DeleteAsync(string id)
        {
            await _meals.DeleteOneAsync(m => m.Id == id);
        }
    }
}

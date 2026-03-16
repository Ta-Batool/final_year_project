using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class CheckInService
    {
        private readonly IMongoCollection<DailyCheckIn> _checkIns;

        public CheckInService(IMongoDatabase db)
        {
            _checkIns = db.GetCollection<DailyCheckIn>("DailyCheckIns");
        }

        // 1 check-in per client per day (UTC)
        public async Task<DailyCheckIn> UpsertAsync(DailyCheckIn input)
        {
            input.DateUtc = input.DateUtc.Date;

            var filter = Builders<DailyCheckIn>.Filter.Where(x =>
                x.ClientId == input.ClientId && x.DateUtc == input.DateUtc);

            var existing = await _checkIns.Find(filter).FirstOrDefaultAsync();
            if (existing is null)
            {
                await _checkIns.InsertOneAsync(input);
                return input;
            }

            input.Id = existing.Id;
            await _checkIns.ReplaceOneAsync(filter, input);
            return input;
        }

        public async Task<List<DailyCheckIn>> GetMonthAsync(string clientId, int year, int month)
        {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var filter = Builders<DailyCheckIn>.Filter.Where(x =>
                x.ClientId == clientId && x.DateUtc >= start && x.DateUtc < end);

            return await _checkIns.Find(filter).SortBy(x => x.DateUtc).ToListAsync();
        }
    }
}

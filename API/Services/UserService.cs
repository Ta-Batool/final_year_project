using MongoDB.Driver;
using Model;
using Microsoft.Extensions.Options;
using API.MongoModel;

namespace API.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);

            _users = database.GetCollection<User>("User");
        }

        // -------------------------
        // Basic CRUD
        // -------------------------

        public async Task<List<User>> GetAllAsync()
        {
            return await _users.Find(u => true).ToListAsync();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(string id, User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }

        // -------------------------
        // ClientId-based helpers
        // -------------------------

        public async Task<User?> GetUserByClientIdAsync(string clientId)
        {
            return await _users.Find(u => u.ClientId == clientId).FirstOrDefaultAsync();
        }

        // from IUserService: UpdateUserByClientIdAsync(string clientId, User updatedUser)
        public async Task UpdateUserByClientIdAsync(string clientId, User updatedUser)
        {
            // get existing user (to keep Id)
            var existingUser = await GetUserByClientIdAsync(clientId);
            if (existingUser == null)
            {
                // you can choose to throw instead
                // throw new InvalidOperationException("User not found for given ClientId");
                return;
            }

            updatedUser.Id = existingUser.Id;

            await _users.ReplaceOneAsync(
                u => u.ClientId == clientId,
                updatedUser
            );
        }

        // from IUserService: UpdateUserAsync(string? id, User updatedUser)
        public async Task UpdateUserAsync(string? id, User updatedUser)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            updatedUser.Id = id;

            await _users.ReplaceOneAsync(u => u.Id == id, updatedUser);
        }
    }
}

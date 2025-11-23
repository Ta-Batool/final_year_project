using MongoDB.Driver;
using Model;
using Microsoft.Extensions.Options;
using API.MongoModel;
using static System.Net.WebRequestMethods;

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
        public async Task UpdateUserByClientIdAsync(string clientId, User updatedUser)
        {
            await _users.ReplaceOneAsync(u => u.ClientId == clientId, updatedUser);
        }
        public async Task<User?> GetUserByClientIdAsync(string clientId)
        {
            return await _users.Find(u => u.ClientId == clientId).FirstOrDefaultAsync();
        }
        public async Task UpdateUserAsync(string? id, User updatedUser)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("User ID cannot be null or empty", nameof(id));

            // Replace the existing document in MongoDB with the updated one
            await _users.ReplaceOneAsync(u => u.Id == id, updatedUser);
        }
       

    }
}

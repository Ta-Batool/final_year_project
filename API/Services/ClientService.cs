using MongoDB.Driver;
using Model;
using Microsoft.Extensions.Options;
using API.MongoModel;

namespace API.Services
{
    public class ClientService : IClientService
    {
        private readonly IMongoCollection<Client> _clients;

        public ClientService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _clients = database.GetCollection<Client>("Client");
            CreateUniqueEmailIndex();
        }

        private void CreateUniqueEmailIndex()
        {
            var indexKeysDefinition = Builders<Client>.IndexKeys.Ascending(c => c.Email);
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<Client>(indexKeysDefinition, indexOptions);
            _clients.Indexes.CreateOne(indexModel);
        }

        public async Task<List<Client>> GetAllAsync()
            => await _clients.Find(c => true).ToListAsync();

        public async Task<Client?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            email = email.ToLowerInvariant();
            return await _clients.Find(c => c.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Client client)
        {
            client.Email = client.Email.ToLowerInvariant();
            await _clients.InsertOneAsync(client);
        }

        public async Task UpdateAsync(string email, Client client)
        {
            email = email.ToLowerInvariant();
            client.Email = email;
            await _clients.ReplaceOneAsync(c => c.Email == email, client);
        }

        public async Task DeleteAsync(string email)
        {
            email = email.ToLowerInvariant();
            await _clients.DeleteOneAsync(c => c.Email == email);
        }

        // ✅ NEW: Set Premium by ClientId
        public async Task<bool> SetPremiumByIdAsync(string clientId, bool isPremium)
        {
            if (string.IsNullOrWhiteSpace(clientId)) return false;

            var update = Builders<Client>.Update.Set(c => c.IsPremium, isPremium);
            var result = await _clients.UpdateOneAsync(c => c.Id == clientId, update);
            return result.ModifiedCount > 0;
        }

        // ✅ NEW: Set Premium by Email
        public async Task<bool> SetPremiumByEmailAsync(string email, bool isPremium)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            email = email.ToLowerInvariant();
            var update = Builders<Client>.Update.Set(c => c.IsPremium, isPremium);
            var result = await _clients.UpdateOneAsync(c => c.Email == email, update);
            return result.ModifiedCount > 0;
        }
    }
}

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

            // Ensure unique email index
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
        {
            return await _clients.Find(c => true).ToListAsync();
        }

        public async Task<Client?> GetByEmailAsync(string email)
        {
            email = email.ToLowerInvariant();
            return await _clients.Find(c => c.Email == email).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Client client)
        {
            client.Email = client.Email.ToLowerInvariant(); // Store email in lowercase
            await _clients.InsertOneAsync(client);
        }

        public async Task UpdateAsync(string email, Client client)
        {
            email = email.ToLowerInvariant();
            client.Email = email; // Ensure updated email is consistent
            await _clients.ReplaceOneAsync(c => c.Email == email, client);
        }

        public async Task DeleteAsync(string email)
        {
            email = email.ToLowerInvariant();
            await _clients.DeleteOneAsync(c => c.Email == email);
        }
    }
}

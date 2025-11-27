using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IMongoCollection<Conversation> _conversations;

        public ConversationService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var db = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _conversations = db.GetCollection<Conversation>("Conversations");
        }

        public async Task<List<Conversation>> GetAllForParticipantAsync(string clientId)
        {
            var filter = Builders<Conversation>.Filter.AnyEq(c => c.ParticipantIds, clientId);
            return await _conversations.Find(filter)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetByIdAsync(string id)
        {
            return await _conversations
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Conversation> CreateAsync(Conversation conversation)
        {
            await _conversations.InsertOneAsync(conversation);
            return conversation;
        }

        public async Task AddParticipantAsync(string conversationId, string clientId)
        {
            var update = Builders<Conversation>.Update.AddToSet(c => c.ParticipantIds, clientId);
            await _conversations.UpdateOneAsync(c => c.Id == conversationId, update);
        }
    }
}
